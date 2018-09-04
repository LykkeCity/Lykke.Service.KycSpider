using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Common.Log;
using Lykke.Common;
using Lykke.Common.Log;
using Lykke.Service.Kyc.Abstractions.Domain.KycDocuments;
using Lykke.Service.Kyc.Abstractions.Domain.KycDocuments.Data;
using Lykke.Service.Kyc.Abstractions.Services;
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
        private readonly ICustomerChecksInfoRepository _customerChecksInfoRepository;
        private readonly ISpiderCheckService _spiderCheckService;
        private readonly ISpiderCheckResultRepository _spiderCheckResultRepository;
        private readonly ISpiderCheckProcessingService _spiderCheckProcessingService;
        private readonly ILog _log;

        private static readonly KycChanger SpiderChanger =
            KycChanger.Service(AppEnvironment.Name, $"{AppEnvironment.Version}/{AppEnvironment.EnvInfo}");

        private static readonly KycChangeRequest UploadRequest = new KycChangeRequest
        {
            Changer = SpiderChanger,
            StatusComment = "Some profiles was changed or added"
        };

        private static readonly GlobalCheckInfo SkippedClientCheckInfo = new GlobalCheckInfo {TotalClients = 1};

        public SpiderRegularCheckService
        (
            IGlobalCheckInfoService globalCheckInfoService,
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            ICustomerChecksInfoRepository customerChecksInfoRepository,
            ISpiderCheckService spiderCheckService,
            ISpiderCheckResultRepository spiderCheckResultRepository,
            ISpiderCheckProcessingService spiderCheckProcessingService,
            ILogFactory logFactory
        )
        {
            _globalCheckInfoService = globalCheckInfoService;
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _customerChecksInfoRepository = customerChecksInfoRepository;
            _spiderCheckService = spiderCheckService;
            _spiderCheckResultRepository = spiderCheckResultRepository;
            _spiderCheckProcessingService = spiderCheckProcessingService;
            _log = logFactory.CreateLog(this);
        }

        public async Task PerformRegularCheckAsync()
        {
            _log.Info("Regular check started");

            var startDateTime = DateTime.UtcNow;
            var stats = new List<IGlobalCheckInfo>();

            var separatingClientId = null as string;

            while (true)
            {
                var customers = (await _customerChecksInfoRepository.GetBatch(100, separatingClientId)).ToArray();

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

            _log.Info("Regular check finished");
        }

        private async Task<GlobalCheckInfo> PerformRegularCheckAsync(IEnumerable<ICustomerChecksInfo> customers)
        {
            var stats = new List<GlobalCheckInfo>();

            foreach (var customer in customers)
            {
                try
                {
                    stats.Add(await CheckCustomer(customer));
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Error at processing customer", customer);
                }
            }

            return SumStatistics(stats);
        }

        private async Task<GlobalCheckInfo> CheckCustomer(ICustomerChecksInfo customer)
        {
            if (!customer.IsPepCheckRequired && !customer.IsCrimeCheckRequired && !customer.IsSanctionCheckRequired)
            {
                return SkippedClientCheckInfo;
            }

            var clientId = customer.CustomerId;

            var currentResult = await _spiderCheckService.CheckAsync(clientId);

            var pepSummary = await CheckCustomerByType(PepSpiderCheck.ApiType, customer, currentResult);
            var crimeSummary = await CheckCustomerByType(CrimeSpiderCheck.ApiType, customer, currentResult);
            var sanctionSummary = await CheckCustomerByType(SanctionSpiderCheck.ApiType, customer, currentResult);

            return CalcClientCheckInfo(pepSummary, crimeSummary, sanctionSummary);
        }

        private async Task<CheckSummary> CheckCustomerByType(string apiType, ICustomerChecksInfo customer, ISpiderCheckResult currentResult)
        {
            if (!customer.GetIsCheckRequired(apiType))
            {
                return CheckSummary.Empty;
            }

            var clientId = customer.CustomerId;
            var previousResultId = customer.GetLatestCheckId(apiType);
            var previousResult = await _spiderCheckResultRepository.GetAsync(clientId, previousResultId);
            var diff = _diffService.ComputeDiff(apiType, currentResult, previousResult);
            var summary = CheckSummary.FromDiff(diff);


            if (summary.IsSuspected)
            {
                var document = await _spiderCheckProcessingService.UploadSpiderCheck(clientId, apiType, UploadRequest);

                await _spiderDocumentInfoRepository.AddOrUpdateAsync(new SpiderDocumentInfo
                {
                    CustomerId = clientId,
                    DocumentId = document.DocumentId,
                    CheckDiff = Mapper.Map<SpiderCheckResultDiff>(diff),
                    CurrentCheckId = currentResult.ResultId,
                    PreviousCheckId = previousResultId
                });

                await _customerChecksInfoRepository.UpdateCheckStatesAsync(clientId, apiType, false);
            }

            await _customerChecksInfoRepository.UpdateLatestCheckIdAsync(clientId, apiType, previousResultId);

            return summary;
        }

        private static GlobalCheckInfo CalcClientCheckInfo(CheckSummary pep, CheckSummary crime, CheckSummary sanction)
        {
            return new GlobalCheckInfo
            {
                PepSuspects = pep.IsSuspected.ToOneOrZero(),
                CrimeSuspects = crime.IsSuspected.ToOneOrZero(),
                SanctionSuspects = sanction.IsSuspected.ToOneOrZero(),

                SpiderChecks = 1,
                TotalClients = 1,

                AddedProfiles = pep.AddedProfiles + crime.AddedProfiles + sanction.AddedProfiles,
                ChangedProfiles = pep.ChangedProfiles + crime.ChangedProfiles + sanction.ChangedProfiles,
                RemovedProfiles = pep.RemovedProfiles + crime.RemovedProfiles + sanction.RemovedProfiles
            };
        }

        private static GlobalCheckInfo SumStatistics(IReadOnlyCollection<IGlobalCheckInfo> stats, DateTime? start = null, DateTime? end = null)
        {
            var added = stats.Sum(x => x.AddedProfiles);
            var removed = stats.Sum(x => x.RemovedProfiles);
            var changed = stats.Sum(x => x.ChangedProfiles);

            return new GlobalCheckInfo
            {
                StartDateTime = start ?? default(DateTime),
                EndDateTime = end ?? default(DateTime),

                TotalClients = stats.Sum(x=> x.TotalClients),
                SpiderChecks = stats.Sum(x => x.SpiderChecks),
                PepSuspects = stats.Sum(x => x.PepSuspects),
                CrimeSuspects = stats.Sum(x => x.CrimeSuspects),
                SanctionSuspects = stats.Sum(x => x.SanctionSuspects),
                TotalProfiles = added + changed + removed,
                AddedProfiles = added,
                RemovedProfiles = removed,
                ChangedProfiles = changed
            };
        }

        private class CheckSummary
        {
            public int AddedProfiles { get; set; }

            public int RemovedProfiles { get; set; }

            public int ChangedProfiles { get; set; }

            public bool IsSuspected => AddedProfiles > 0 && RemovedProfiles > 0 && ChangedProfiles > 0;

            public static CheckSummary FromDiff(ISpiderCheckResultDiff diff)
            {
                return new CheckSummary
                {
                    AddedProfiles = diff.AddedProfiles.Count,
                    RemovedProfiles = diff.RemovedProfiles.Count,
                    ChangedProfiles = diff.ChangedProfiles.Count
                };
            }

            public static CheckSummary Empty => new CheckSummary();
        }
    }
}
