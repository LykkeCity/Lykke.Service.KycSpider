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
using System.Threading.Tasks;
using Lykke.Service.Kyc.Abstractions.Domain.Documents.V3;
using System;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderFirstCheckService : ISpiderFirstCheckService
    {
        private readonly ICheckPersonResultDiffService _diffService;
        private readonly ISpiderDocumentInfoRepository _spiderDocumentInfoRepository;
        private readonly ICustomerChecksInfoRepository _customerChecksRepository;
        private readonly ISpiderCheckService _spiderCheckService;
        private readonly ISpiderCheckProcessingService _spiderCheckProcessingService;
        private readonly ISpiderCheckResultRepository _checkResultRepository;
        private readonly ILog _log;

        private static readonly KycChanger SpiderChanger =
            KycChanger.Service(AppEnvironment.Name, $"{AppEnvironment.Version}/{AppEnvironment.EnvInfo}");
        
        private const string AutoApprovedStatusComment = "No suspected profiles";
        private const string UploadedStatusComment = "There are suspected profiles";

        public SpiderFirstCheckService
        (
            ICheckPersonResultDiffService diffService,
            ISpiderDocumentInfoRepository spiderDocumentInfoRepository,
            ICustomerChecksInfoRepository customerChecksRepository,
            ISpiderCheckService spiderCheckService,
            ISpiderCheckProcessingService spiderCheckProcessingService,
            ISpiderCheckResultRepository checkResultRepository,
            ILogFactory logFactory
        )
        {
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _customerChecksRepository = customerChecksRepository;
            _spiderCheckService = spiderCheckService;
            _spiderCheckProcessingService = spiderCheckProcessingService;
            _checkResultRepository = checkResultRepository;
            _log = logFactory.CreateLog(this);
        }

        public async Task<SpiderDocumentAutoStatusGroup> PerformFirstCheckAsync(string clientId, ISpiderCheckResult spiderResult = null)
        {
            _log.Info($"Stated first spider check for {clientId}");

            var checkResult = await GetCheckResult(clientId, spiderResult);
            var checksInfo = await SaveDocuments(checkResult);
            await SaveCustomerChecksInfo(checkResult, checksInfo);

            _log.Info($"Finished first spider check for {clientId}");

            return checksInfo;
        }

        private async Task<ISpiderCheckResult> GetCheckResult(string clientId, ISpiderCheckResult spiderResult)
        {
            if (spiderResult != null)
            {
                return await _checkResultRepository.AddAsync(spiderResult);
            }

            return await _spiderCheckService.CheckAsync(clientId);
        }

        private async Task SaveCustomerChecksInfo(ISpiderCheckResult result, SpiderDocumentAutoStatusGroup checksInfo)
        {
            var clientId = result.CustomerId;
            var checkId = result.ResultId;

            await _customerChecksRepository.AddAsync(new CustomerChecksInfo
            {
                CustomerId = clientId,

                LatestPepCheckId = checkId,
                LatestCrimeCheckId = checkId,
                LatestSanctionCheckId = checkId,

                IsPepCheckRequired = checksInfo.Pep.IsAutoApproved,
                IsCrimeCheckRequired = checksInfo.Crime.IsAutoApproved,
                IsSanctionCheckRequired = checksInfo.Sanction.IsAutoApproved
            });
        }

        private async Task<SpiderDocumentAutoStatusGroup> SaveDocuments(ISpiderCheckResult result)
        {
            var pepDiff = _diffService.ComputeDiffWithEmptyByPep(result);
            var crimeDiff = _diffService.ComputeDiffWithEmptyByCrime(result);
            var sanctionDiff = _diffService.ComputeDiffWithEmptyBySanction(result);

            var clientId = result.CustomerId;
            var checkId = result.ResultId;

            var pepTask = SaveDocument(clientId, pepDiff, PepSpiderCheck.ApiType, checkId);
            var crimeTask = SaveDocument(clientId, crimeDiff, CrimeSpiderCheck.ApiType, checkId);
            var sanctionTask = SaveDocument(clientId, sanctionDiff, SanctionSpiderCheck.ApiType, checkId);

            return new SpiderDocumentAutoStatusGroup
            {
                Pep = await pepTask,
                Crime = await crimeTask,
                Sanction = await sanctionTask
            };
        }

        private async Task<SpiderDocumentAutoStatus> SaveDocument(string clientId, ISpiderCheckResultDiff diff, string type, string checkId)
        {
            var isAutoApprovedDiff = IsAutoApprovedDiff(diff);
            var document = await SaveKycDocumentInfo(clientId, type, isAutoApprovedDiff);
            var documentId = document.DocumentId;

            await SaveSpiderDocumentInfo(clientId, documentId, diff, checkId);

            return new SpiderDocumentAutoStatus
            {
                ApiType = type,
                DocumentId = documentId,
                IsAutoApproved = isAutoApprovedDiff
            };
        }

        private async Task<IKycDocumentInfo> SaveKycDocumentInfo(string clientId, string type, bool isAutoApprovedDiff)
        {
            if (isAutoApprovedDiff)
            {
                return await _spiderCheckProcessingService.AddApprovedSpiderCheck(clientId, type,
                    new KycChangeRequest<CommonSpiderCheck>
                    {
                        Changer = SpiderChanger,
                        StatusComment = AutoApprovedStatusComment,
                        Info = new CommonSpiderCheck
                        {
                            CheckResultSatisfaction = true,
                            Resolution = AutoApprovedStatusComment
                        }
                    }
                );
            }

            return await _spiderCheckProcessingService.UploadSpiderCheck(clientId, type, new KycChangeRequest
            {
                Changer = SpiderChanger,
                StatusComment = UploadedStatusComment
            });
        }


        private async Task SaveSpiderDocumentInfo(string clientId, string documentId, ISpiderCheckResultDiff diff, string checkId)
        {
            await _spiderDocumentInfoRepository.AddOrUpdateAsync(new SpiderDocumentInfo
            {
                CustomerId = clientId,
                DocumentId = documentId,
                CheckDiff = Mapper.Map<SpiderCheckResultDiff>(diff),
                CurrentCheckId = checkId,
                PreviousCheckId = null
            });
        }

        private static bool IsAutoApprovedDiff(ISpiderCheckResultDiff diff)
        {
            return diff.AddedProfiles.Count == 0 && diff.ChangedProfiles.Count == 0;
        }
    }
}
