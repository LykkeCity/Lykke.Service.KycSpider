using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Service.KycSpider.Core.Domain.PersonDiff;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;
using Lykke.Service.PersonalData.Client.Models;
using Lykke.Service.PersonalData.Client.Models.Documents;
using Lykke.Service.PersonalData.Contract;
using Lykke.Service.PersonalData.Contract.Models.Documents;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderRegularCheckService : ISpiderRegularCheckService
    {
        private readonly TimeSpan _dailyCheckTimeUtc;

        private readonly IGlobalCheckInfoService _globalCheckInfoService;
        private readonly ICheckPersonResultDiffService _diffService;
        private readonly ISpiderDocumentInfoRepository _spiderDocumentInfoRepository;
        private readonly IVerifiableCustomerInfoRepository _verifiableCustomerRepository;
        private readonly ITypedDocumentsService _typedDocumentsService;
        private readonly ISpiderCheckService _spiderCheckService;
        private readonly ISpiderCheckResultRepository _spiderCheckResultRepository;
        private readonly ILog _log;

        public SpiderRegularCheckService
        (
            SpiderCheckSettings settings,
            IGlobalCheckInfoService globalCheckInfoService,
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            IVerifiableCustomerInfoRepository verifiableCustomerRepository,
            ITypedDocumentsService typedDocumentsService,
            ISpiderCheckService spiderCheckService,
            ISpiderCheckResultRepository spiderCheckResultRepository,
            ILog log           
        )
        {
            _dailyCheckTimeUtc = settings.DailyCheckTimeUtc;
            _globalCheckInfoService = globalCheckInfoService;
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _verifiableCustomerRepository = verifiableCustomerRepository;
            _typedDocumentsService = typedDocumentsService;
            _spiderCheckService = spiderCheckService;
            _spiderCheckResultRepository = spiderCheckResultRepository;
            _log = log;

            if (_dailyCheckTimeUtc < TimeSpan.FromHours(0) || TimeSpan.FromHours(24) <= _dailyCheckTimeUtc)
            {
                throw new ArgumentException($"Incorrect time of day at {nameof(settings)}.{nameof(settings.DailyCheckTimeUtc)}", nameof(settings));
            }
        }

        private Task _regularCheckTask;
        private DateTime _regularCheckStartDateTime;
        private const int RegularCheckHoursDurationToWarn = 20;
        private bool _regularCheckWarningLogged;

        public async Task PerformCheckAsync()
        {
            if (await IsDailyCheckShouldBePerformedNow())
            {
                if (_regularCheckTask == null)
                {
                    _regularCheckTask = PerformRegularCheckAsync();
                    _regularCheckStartDateTime = DateTime.UtcNow;
                    _regularCheckWarningLogged = false;
                }
                else if (_regularCheckTask.IsCompleted)
                {
                    var task = _regularCheckTask;
                    _regularCheckTask = null;
                    await task;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    if (_regularCheckStartDateTime.AddHours(RegularCheckHoursDurationToWarn) < now && !_regularCheckWarningLogged)
                    {
                        await _log.WriteWarningAsync(nameof(SpiderRegularCheckService), nameof(PerformCheckAsync),
                            $"Regular check lasts more then {RegularCheckHoursDurationToWarn} hours is starts at {_regularCheckStartDateTime}");
                        _regularCheckWarningLogged = true;
                    }
                }
            }

            await PerformInstantCheckAsync();
        }
        
        private async Task<bool> IsDailyCheckShouldBePerformedNow()
        {
            var now = DateTime.UtcNow;

            if (now.TimeOfDay <= _dailyCheckTimeUtc)
            {
                return false;
            }

            var latestCheckTimestamp = await _globalCheckInfoService.GetLatestCheckTimestamp();

            return !latestCheckTimestamp.HasValue || latestCheckTimestamp.Value < now.Date;
        }

        private async Task PerformInstantCheckAsync()
        {
            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformInstantCheckAsync), "started");

            var pepDocs = await _typedDocumentsService.GetAllPepCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);
            var crimeDocs = await _typedDocumentsService.GetAllCrimeCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);
            var sanctionDocs = await _typedDocumentsService.GetAllSanctionCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);

            var clientDict = pepDocs
                .Select(x => x.CustomerId)
                .Concat(crimeDocs.Select(x => x.CustomerId))
                .Concat(sanctionDocs.Select(x => x.CustomerId))
                .Distinct()
                .ToDictionary(x => x, x => new InstantCheckState());

            foreach (var pepDoc in pepDocs)
            {
                clientDict[pepDoc.CustomerId].Pep = pepDoc;
            }

            foreach (var crimeDoc in crimeDocs)
            {
                clientDict[crimeDoc.CustomerId].Crime = crimeDoc;
            }

            foreach (var sanctionDoc in sanctionDocs)
            {
                clientDict[sanctionDoc.CustomerId].Sanction = sanctionDoc;
            }

            foreach (var clientId in clientDict.Keys)
            {
                var state = clientDict[clientId];

                var spiderCheckId = (await _verifiableCustomerRepository.GetAsync(clientId))?.LatestSpiderCheckId;

                if (spiderCheckId != null)
                {
                    state.CheckResult = await _spiderCheckResultRepository.GetAsync(clientId, spiderCheckId);
                }
                else
                {
                    state.CheckResult = await _spiderCheckService.CheckAsync(clientId);
                }

                await FormAndUpdateDocuments(state);
                await UpdateVerifiableCustomer(state);
            }

            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformInstantCheckAsync), "done");
        }

        private class InstantCheckState
        {
            public ISpiderCheckResult CheckResult { get; set; }
            public PepCheckDocument Pep { get; set; }
            public CrimeCheckDocument Crime { get; set; }
            public SanctionCheckDocument Sanction { get; set; }
        }

        private async Task UpdateVerifiableCustomer(InstantCheckState state)
        {
            var clientId = state.CheckResult.CustomerId;
            var oldClient = await _verifiableCustomerRepository.GetAsync(clientId) ??
                            new VerifiableCustomerInfo {CustomerId = clientId};

            var newClient = new VerifiableCustomerInfo
            {
                CustomerId = oldClient.CustomerId,
                IsCrimeCheckRequired = oldClient.IsCrimeCheckRequired || state.Crime != null,
                IsSanctionCheckRequired = oldClient.IsSanctionCheckRequired || state.Sanction != null,
                IsPepCheckRequired = oldClient.IsPepCheckRequired || state.Pep != null,
                LatestSpiderCheckId = state.CheckResult.ResultId
            };

            await _verifiableCustomerRepository.AddOrUpdateAsync(newClient);
        }

        private async Task FormAndUpdateDocuments(InstantCheckState state)
        {
            var currentResultId = state.CheckResult.ResultId;

            if (state.Pep != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByPep(state.CheckResult);
                var doc = state.Pep;

                if (IsSuspectedDiff(diff))
                {
                    doc.State = DocumentStates.Uploaded;
                }
                else
                {
                    doc.State = DocumentStates.Approved;
                    doc.CheckResultSatisfaction = true;
                }

                doc.CheckDateTime = DateTime.UtcNow;

                await _typedDocumentsService.AddOrUpdatePepCheckDocumentAsync(doc);
                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }

            if (state.Crime != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByCrime(state.CheckResult);
                var doc = state.Crime;

                if (IsSuspectedDiff(diff))
                {
                    doc.State = DocumentStates.Uploaded;
                }
                else
                {
                    doc.State = DocumentStates.Approved;
                    doc.CheckResultSatisfaction = true;
                }

                doc.CheckDateTime = DateTime.UtcNow;

                await _typedDocumentsService.AddOrUpdateCrimeCheckDocumentAsync(doc);
                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }

            if (state.Sanction != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyBySanction(state.CheckResult);
                var doc = state.Sanction;

                if (IsSuspectedDiff(diff))
                {
                    doc.State = DocumentStates.Uploaded;
                }
                else
                {
                    doc.State = DocumentStates.Approved;
                    doc.CheckResultSatisfaction = true;
                }

                doc.CheckDateTime = DateTime.UtcNow;

                await _typedDocumentsService.AddOrUpdateSanctionCheckDocumentAsync(doc);
                await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(currentResultId, diff, doc));
            }
        }

        private async Task PerformRegularCheckAsync()
        {
            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformRegularCheckAsync), "started");

            var startDateTime = DateTime.UtcNow;
            var stats = new List<IGlobalCheckInfo>();

            var separatingClientId = null as string;

            while (true)
            {
                var customers = (await _verifiableCustomerRepository.GetBatch(100, separatingClientId)).ToArray();

                if (!customers.Any())
                {
                    break;
                }

                separatingClientId = customers.Last().CustomerId;

                stats.Add(await PerformRegularCheckAsync(customers));
            }

            var endDateTime = DateTime.UtcNow;
            var checkInfo = SumStatistics(stats, startDateTime, endDateTime);
            await _globalCheckInfoService.AddCheckInfo(checkInfo);
            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformRegularCheckAsync), "done");
        }

        private async Task<IGlobalCheckInfo> PerformRegularCheckAsync(IEnumerable<IVerifiableCustomerInfo> customers)
        {
            var stats = new List<IGlobalCheckInfo>();

            foreach (var customer in customers)
            {
                var request = new PersonDiffRequest
                {
                    Customer = customer,
                    CurrentResult = await _spiderCheckService.CheckAsync(customer.CustomerId),
                    PreviousResult = await _spiderCheckResultRepository.GetAsync(customer.CustomerId, customer.LatestSpiderCheckId)
                };

                var result = _diffService.ComputeAllDiffs(request);

                await FormAndSaveDocuments(request, result);

                stats.Add(ComputeStatistics(request, result));
            }

            return SumStatistics(stats);
        }

        private async Task FormAndSaveDocuments(IPersonDiffRequest request, IPersonDiffResult result)
        {
            if (request.Customer.IsPepCheckRequired)
            {
                var diff = result.PepDiff;

                if (IsSuspectedDiff(diff))
                {
                    var doc = await _typedDocumentsService.AddOrUpdatePepCheckDocumentAsync(new PepCheckDocument
                    {
                        CustomerId = request.Customer.CustomerId,
                        CheckDateTime = DateTime.UtcNow,
                        State = DocumentStates.Uploaded
                    });

                    await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(request, diff, doc));
                }
            }

            if (request.Customer.IsCrimeCheckRequired)
            {
                var diff = result.CrimeDiff;

                if (IsSuspectedDiff(diff))
                {
                    var doc = await _typedDocumentsService.AddOrUpdateCrimeCheckDocumentAsync(new CrimeCheckDocument
                    {
                        CustomerId = request.Customer.CustomerId,
                        CheckDateTime = DateTime.UtcNow,
                        State = DocumentStates.Uploaded
                    });

                    await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(request, diff, doc));
                }
            }

            if (request.Customer.IsSanctionCheckRequired)
            {
                var diff = result.SanctionDiff;

                if (IsSuspectedDiff(diff))
                {
                    var doc = await _typedDocumentsService.AddOrUpdateSanctionCheckDocumentAsync(new SanctionCheckDocument
                    {
                        CustomerId = request.Customer.CustomerId,
                        CheckDateTime = DateTime.UtcNow,
                        State = DocumentStates.Uploaded
                    });

                    await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(request, diff, doc));
                }
            }
        }

        private static ISpiderDocumentInfo FormSpiderDocumentInfo(IPersonDiffRequest request, ISpiderCheckResultDiff diff, IKycDocumentInfo doc)
        {
            return new SpiderDocumentInfo
            {
                CustomerId = doc.CustomerId,
                DocumentId = doc.DocumentId,
                CheckDiff = Mapper.Map<SpiderCheckResultDiff>(diff),
                CurrentCheckId = request.CurrentResult.ResultId,
                PreviousCheckId = request.CurrentResult.ResultId
            };
        }

        private static ISpiderDocumentInfo FormSpiderDocumentInfo(string currentCheckId, ISpiderCheckResultDiff diff, IKycDocumentInfo doc)
        {
            return new SpiderDocumentInfo
            {
                CustomerId = doc.CustomerId,
                DocumentId = doc.DocumentId,
                CheckDiff = Mapper.Map<SpiderCheckResultDiff>(diff),
                CurrentCheckId = currentCheckId,
                PreviousCheckId = null
            };
        }

        private static IGlobalCheckInfo ComputeStatistics(IPersonDiffRequest request, IPersonDiffResult result)
        {
            return new GlobalCheckInfo
            {
                SpiderChecks = 1,
                PepSuspects = IsSuspectedDiff(result.PepDiff)? 1 : 0,
                CrimeSuspects = IsSuspectedDiff(result.CrimeDiff) ? 1 : 0,
                SanctionSuspects = IsSuspectedDiff(result.SanctionDiff) ? 1 : 0,
                TotalProfiles = request.CurrentResult.PersonProfiles.Count,
                AddedProfiles = result.PepDiff.AddedProfiles.Count + result.CrimeDiff.AddedProfiles.Count + result.SanctionDiff.AddedProfiles.Count,
                RemovedProfiles = result.PepDiff.RemovedProfiles.Count + result.CrimeDiff.RemovedProfiles.Count + result.SanctionDiff.RemovedProfiles.Count,
                ChangedProfiles = result.PepDiff.ChangedProfiles.Count + result.CrimeDiff.ChangedProfiles.Count + result.SanctionDiff.ChangedProfiles.Count
            };
        }

        private static IGlobalCheckInfo SumStatistics(IReadOnlyCollection<IGlobalCheckInfo> stats, DateTime? start = null, DateTime? end = null)
        {
            return new GlobalCheckInfo
            {
                StartDateTime = start ?? default(DateTime),
                EndDateTime = end ?? default(DateTime),

                SpiderChecks = stats.Sum(x => x.SpiderChecks),
                PepSuspects = stats.Sum(x => x.PepSuspects),
                CrimeSuspects = stats.Sum(x => x.CrimeSuspects),
                SanctionSuspects = stats.Sum(x => x.SanctionSuspects),
                TotalProfiles = stats.Sum(x => x.TotalProfiles),
                AddedProfiles = stats.Sum(x => x.AddedProfiles),
                RemovedProfiles = stats.Sum(x => x.RemovedProfiles),
                ChangedProfiles = stats.Sum(x => x.ChangedProfiles)
            };
        }

        private static bool IsSuspectedDiff(ISpiderCheckResultDiff diff)
        {
            return diff.AddedProfiles.Count > 0 || diff.ChangedProfiles.Count > 0;
        }
    }
}
