using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.Kyc.Abstractions.Domain.KycDocuments.Contract;
using Lykke.Service.Kyc.Abstractions.Requests;
using Lykke.Service.Kyc.Abstractions.Services;
using Lykke.Service.KycSpider.Core.Domain.PersonDiff;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderRegularCheckService : ISpiderRegularCheckService
    {
        private readonly IGlobalCheckInfoService _globalCheckInfoService;
        private readonly ICheckPersonResultDiffService _diffService;
        private readonly ISpiderDocumentInfoRepository _spiderDocumentInfoRepository;
        private readonly IVerifiableCustomerInfoRepository _verifiableCustomerRepository;
        private readonly ISpiderCheckService _spiderCheckService;
        private readonly ISpiderCheckResultRepository _spiderCheckResultRepository;
        private readonly IRequestableDocumentsService _requestableDocumentsService;
        private readonly ILog _log;

        private static readonly Changer SpiderChanger = new Changer
        {
            Name = "Spider"
        };

        public SpiderRegularCheckService
        (
            IGlobalCheckInfoService globalCheckInfoService,
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            IVerifiableCustomerInfoRepository verifiableCustomerRepository,
            ISpiderCheckService spiderCheckService,
            ISpiderCheckResultRepository spiderCheckResultRepository,
            IRequestableDocumentsService requestableDocumentsService,
            ILogFactory logFactory
        )
        {
            _globalCheckInfoService = globalCheckInfoService;
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _verifiableCustomerRepository = verifiableCustomerRepository;
            _spiderCheckService = spiderCheckService;
            _spiderCheckResultRepository = spiderCheckResultRepository;
            _requestableDocumentsService = requestableDocumentsService;
            _log = logFactory.CreateLog(this);
        }

        public async Task PerformRegularCheckAsync()
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

                await SaveDocuments(request, result);
                await _verifiableCustomerRepository.UpdateSpiderCheckIdAsync(customer.CustomerId, request.CurrentResult.ResultId);

                stats.Add(ComputeStatistics(request, result));
            }

            return SumStatistics(stats);
        }

        private async Task SaveDocuments(IPersonDiffRequest request, IPersonDiffResult result)
        {
            if (request.Customer.IsPepCheckRequired)
            {
                var diff = result.PepDiff;

                if (IsSuspectedDiff(diff))
                {
                    var doc = await _requestableDocumentsService.RequestDocumentFromOfficerAsync(
                        request.Customer.CustomerId,
                        DocumentTypes.PepCheckDocument,
                        SpiderChanger);

                    await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(SaveDocuments),
                        $"Client {doc.CustomerId} is suspected for pep created new document (DocumentId: {doc.DocumentId})");
                    await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(request, diff, doc));
                }
                else
                {
                    await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(SaveDocuments),
                        $"Client {request.Customer.CustomerId} is not suspected for pep");
                }
            }

            if (request.Customer.IsCrimeCheckRequired)
            {
                var diff = result.CrimeDiff;

                if (IsSuspectedDiff(diff))
                {
                    var doc = await _requestableDocumentsService.RequestDocumentFromOfficerAsync(
                        request.Customer.CustomerId,
                        DocumentTypes.CrimeCheckDocument,
                        SpiderChanger);

                    await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(SaveDocuments),
                        $"Client {doc.CustomerId} is suspected for crime created new document (DocumentId: {doc.DocumentId})");
                    await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(request, diff, doc));
                }
                else
                {
                    await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(SaveDocuments),
                        $"Client {request.Customer.CustomerId} is not suspected for crime");
                }
            }

            if (request.Customer.IsSanctionCheckRequired)
            {
                var diff = result.SanctionDiff;

                if (IsSuspectedDiff(diff))
                {
                    var doc = await _requestableDocumentsService.RequestDocumentFromOfficerAsync(
                        request.Customer.CustomerId,
                        DocumentTypes.SanctionCheckDocument,
                        SpiderChanger);

                    await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(SaveDocuments),
                        $"Client {doc.CustomerId} is suspected for sanction created new document (DocumentId: {doc.DocumentId})");
                    await _spiderDocumentInfoRepository.AddOrUpdateAsync(FormSpiderDocumentInfo(request, diff, doc));
                }
                else
                {
                    await _log.WriteInfoAsync(nameof(SpiderRegularCheckService), nameof(SaveDocuments),
                        $"Client {request.Customer.CustomerId} is not suspected for sanction");
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
                PreviousCheckId = request.PreviousResult.ResultId
            };
        }

        private static IGlobalCheckInfo ComputeStatistics(IPersonDiffRequest request, IPersonDiffResult result)
        {
            return new GlobalCheckInfo
            {
                SpiderChecks = 1,
                PepSuspects = IsSuspectedDiff(result.PepDiff) ? 1 : 0,
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
