using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.KycSpider.Core.Domain.CheckPerson;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
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
        private readonly IKycStatusService _statusService;
        private readonly IKycCheckPersonService _checkPersonService;
        private readonly ILog _log;

        public SpiderRegularCheckService
        (
            SpiderCheckSettings settings,
            IGlobalCheckInfoService globalCheckInfoService,
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            IVerifiableCustomerInfoRepository verifiableCustomerRepository,
            ITypedDocumentsService typedDocumentsService,
            IKycStatusService statusService,
            IKycCheckPersonService checkPersonService,
            ILog log           
        )
        {
            _dailyCheckTimeUtc = settings.DailyCheckTimeUtc;
            _globalCheckInfoService = globalCheckInfoService;
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _verifiableCustomerRepository = verifiableCustomerRepository;
            _typedDocumentsService = typedDocumentsService;
            _statusService = statusService;
            _checkPersonService = checkPersonService;
            _log = log;

            if (_dailyCheckTimeUtc < TimeSpan.FromHours(0) || TimeSpan.FromHours(24) <= _dailyCheckTimeUtc)
            {
                throw new ArgumentException("Incorrect time of day", nameof(settings.DailyCheckTimeUtc));
            }
        }

        public async Task PerformCheckAsync()
        {
            if (await IsDailyCheckShouldBePerformedNow())
            {
                await PerformDailyCheck();
            }

            await PerformInstantCheck();
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

        private async Task PerformInstantCheck()
        {
            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformInstantCheck), "started");

            var pepDocs = await _typedDocumentsService.GetAllPepCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);
            var crimeDocs = await _typedDocumentsService.GetAllCrimeCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);
            var sanctionDocs = await _typedDocumentsService.GetAllSanctionCheckDocumentsByStateAsync(DocumentStates.Draft, null, null);

            var clientDict = pepDocs
                .Select(x => x.CustomerId)
                .Concat(crimeDocs.Select(x => x.CustomerId))
                .Concat(sanctionDocs.Select(x => x.CustomerId))
                .Distinct()
                .ToDictionary(x => x, x => (CheckResult: null as IKycCheckPersonResult,
                    Pep: null as PepCheckDocument,
                    Crime: null as CrimeCheckDocument,
                    Sanction: null as SanctionCheckDocument));

            foreach (var pepDoc in pepDocs)
            {
                var batch = clientDict[pepDoc.CustomerId];
                batch.Pep = pepDoc;
                clientDict[pepDoc.CustomerId] = batch;
            }

            foreach (var crimeDoc in crimeDocs)
            {
                var batch = clientDict[crimeDoc.CustomerId];
                batch.Crime = crimeDoc;
                clientDict[crimeDoc.CustomerId] = batch;
            }

            foreach (var sanctionDoc in sanctionDocs)
            {
                var batch = clientDict[sanctionDoc.CustomerId];
                batch.Sanction = sanctionDoc;
                clientDict[sanctionDoc.CustomerId] = batch;
            }

            foreach (var clientId in clientDict.Keys)
            {
                var batch = clientDict[clientId];
                batch.CheckResult = await _statusService.GetCheckPersonResultAsync(clientId) ??
                                    await _checkPersonService.CheckPersonAsync(clientId);

                clientDict[clientId] = batch;

                await FormAndUpdateDocuments(batch);
                await UpdateVerifiableCustomer(clientId, batch);
            }

            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformInstantCheck), "done");
        }

        private async Task UpdateVerifiableCustomer(string clientId, (IKycCheckPersonResult CheckResult, PepCheckDocument Pep, CrimeCheckDocument Crime, SanctionCheckDocument Sanction) batch)
        {
            var oldClient = await _verifiableCustomerRepository.GetAsync(clientId) ??
                            new VerifiableCustomerInfo {CustomerId = clientId};

            var newClient = new VerifiableCustomerInfo
            {
                CustomerId = oldClient.CustomerId,
                IsCrimeCheckRequired = oldClient.IsCrimeCheckRequired || batch.Crime != null,
                IsSanctionCheckRequired = oldClient.IsSanctionCheckRequired || batch.Sanction != null,
                IsPepCheckRequired = oldClient.IsPepCheckRequired || batch.Pep != null,
                LatestSpiderCheckId = batch.CheckResult.Id
            };

            await _verifiableCustomerRepository.AddOrUpdateAsync(newClient);
        }

        private async Task FormAndUpdateDocuments((IKycCheckPersonResult CheckResult, PepCheckDocument Pep, CrimeCheckDocument Crime, SanctionCheckDocument Sanction) batch)
        {
            var currentResultId = batch.CheckResult.Id;

            if (batch.Pep != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByPep(batch.CheckResult);
                var doc = batch.Pep;

                if (IsSuspected(diff))
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

            if (batch.Crime != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyByCrime(batch.CheckResult);
                var doc = batch.Crime;

                if (IsSuspected(diff))
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

            if (batch.Sanction != null)
            {
                var diff = _diffService.ComputeDiffWithEmptyBySanction(batch.CheckResult);
                var doc = batch.Sanction;

                if (IsSuspected(diff))
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

        private async Task PerformDailyCheck()
        {
            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformDailyCheck), "started");

            var stats = new List<IGlobalCheckInfo>(LowHex2Range.Count());

            foreach (var firstByte in LowHex2Range)
            {
                var customers = new List<IVerifiableCustomerInfo>();

                foreach (var secondByte in LowHex2Range)
                {
                    var customersPart = await _verifiableCustomerRepository.GetPartitionAsync(firstByte + secondByte);

                    customers.AddRange(customersPart);
                }

                stats.Add(await PerformDailyCheck(customers));
            }

            await _globalCheckInfoService.AddCheckInfo(SumStatistics(stats));
            await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(PerformDailyCheck), "done");
        }

        private async Task<IGlobalCheckInfo> PerformDailyCheck(IReadOnlyCollection<IVerifiableCustomerInfo> customers)
        {
            var previousCheckIdentities = customers
                .Select(x => new KycCheckPersonResultIdentity
                {
                    ClientId = x.CustomerId,
                    ResultId = x.LatestSpiderCheckId
                });

            var previousCheckResults = await _statusService.GetCheckPersonResultByIdsAsync(previousCheckIdentities);
            var previousCheckResultsQueue = new Queue<IKycCheckPersonResult>(previousCheckResults);

            var diffRequests = new List<IGlobalCheckPersonRequest>(previousCheckResults.Count);

            foreach (var customer in customers)
            {
                var currentResult = await _checkPersonService.CheckPersonAsync(customer.CustomerId);

                diffRequests.Add(new GlobalCheckPersonRequest
                {
                    Customer = customer,
                    CurrentResult = currentResult,
                    PreviousResult = previousCheckResultsQueue.Dequeue()
                });
            }

            var diffResults = await _diffService.ComputeAllDiffsAsync(diffRequests);

            foreach (var result in diffResults)
            {
                await FormAndSaveDocuments(result);
            }

            return ComputeStatistics(diffRequests, diffResults);
        }

        private async Task FormAndSaveDocuments(IGlobalCheckPersonResult result)
        {
            var request = result.Request;

            if (request.Customer.IsPepCheckRequired)
            {
                var diff = result.PepDiff;

                if (IsSuspected(diff))
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

                if (IsSuspected(diff))
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

                if (IsSuspected(diff))
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

        private static ISpiderDocumentInfo FormSpiderDocumentInfo(IGlobalCheckPersonRequest request, ICheckPersonResultDiff diff, IKycDocumentInfo doc)
        {
            return new SpiderDocumentInfo
            {
                CustomerId = doc.CustomerId,
                DocumentId = doc.DocumentId,
                CheckDiff = diff,
                CurrentCheckId = request.CurrentResult.Id,
                PreviousCheckId = request.CurrentResult.Id
            };
        }

        private static ISpiderDocumentInfo FormSpiderDocumentInfo(string currentCheckId, ICheckPersonResultDiff diff, IKycDocumentInfo doc)
        {
            return new SpiderDocumentInfo
            {
                CustomerId = doc.CustomerId,
                DocumentId = doc.DocumentId,
                CheckDiff = diff,
                CurrentCheckId = currentCheckId,
                PreviousCheckId = null
            };
        }

        private static IGlobalCheckInfo ComputeStatistics(IReadOnlyCollection<IGlobalCheckPersonRequest> diffRequests, IReadOnlyCollection<IGlobalCheckPersonResult> diffs)
        {
            return new GlobalCheckInfo
            {
                SpiderChecks = diffRequests.Count,
                PepSuspects = diffs.Count(x => IsSuspected(x.PepDiff)),
                CrimeSuspects = diffs.Count(x => IsSuspected(x.CrimeDiff)),
                SanctionSuspects = diffs.Count(x => IsSuspected(x.SanctionDiff)),
                TotalProfiles = diffRequests.Sum(x => x.CurrentResult.PersonProfiles.Count()),
                AddedProfiles = diffs.Sum(x => x.PepDiff.AddedProfiles.Count + x.CrimeDiff.AddedProfiles.Count + x.SanctionDiff.AddedProfiles.Count),
                RemovedProfiles = diffs.Sum(x => x.PepDiff.RemovedProfiles.Count + x.CrimeDiff.RemovedProfiles.Count + x.SanctionDiff.RemovedProfiles.Count),
                ChangedProfiles = diffs.Sum(x => x.PepDiff.ChangedProfiles.Count + x.CrimeDiff.ChangedProfiles.Count + x.SanctionDiff.ChangedProfiles.Count)
            };
        }

        private static IGlobalCheckInfo SumStatistics(IReadOnlyCollection<IGlobalCheckInfo> stats)
        {
            return new GlobalCheckInfo
            {
                SpiderChecks = stats.Sum(x=> x.SpiderChecks),
                PepSuspects = stats.Sum(x => x.PepSuspects),
                CrimeSuspects = stats.Sum(x => x.CrimeSuspects),
                SanctionSuspects = stats.Sum(x => x.SanctionSuspects),
                TotalProfiles = stats.Sum(x => x.TotalProfiles),
                AddedProfiles = stats.Sum(x => x.AddedProfiles),
                RemovedProfiles = stats.Sum(x => x.RemovedProfiles),
                ChangedProfiles = stats.Sum(x => x.ChangedProfiles)
            };
        }

        private static bool IsSuspected(ICheckPersonResultDiff diff)
        {
            return diff.AddedProfiles.Count > 0 || diff.ChangedProfiles.Count > 0;
        }

        private readonly static IEnumerable<string> LowHex2Range = Enumerable.Range(0, 255).Select(x => x.ToString("x2")).ToArray();
    }
}

