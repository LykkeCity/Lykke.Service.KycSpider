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

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderFirstCheckService : ISpiderFirstCheckService
    {
        private readonly ICheckPersonResultDiffService _diffService;
        private readonly ISpiderDocumentInfoRepository _spiderDocumentInfoRepository;
        private readonly ICustomerChecksInfoRepository _customerChecksRepository;
        private readonly ISpiderCheckService _spiderCheckService;
        private readonly ISpiderCheckProcessingService _spiderCheckProcessingService;
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
            ILogFactory logFactory
        )
        {
            _diffService = diffService;
            _spiderDocumentInfoRepository = spiderDocumentInfoRepository;
            _customerChecksRepository = customerChecksRepository;
            _spiderCheckService = spiderCheckService;
            _spiderCheckProcessingService = spiderCheckProcessingService;
            _log = logFactory.CreateLog(this);
        }

        public async Task PerformFirstCheckAsync(string clientId)
        {
            _log.Info($"Stated first spider check for {clientId}");

            var checkResult = await _spiderCheckService.CheckAsync(clientId);
            var checksInfo = await SaveDocuments(checkResult);
            await SaveCustomerChecksInfo(checkResult, checksInfo);

            _log.Info($"Finished first spider check for {clientId}");
        }

        private async Task SaveCustomerChecksInfo(ISpiderCheckResult result, (bool pep, bool crime, bool sanction) checksInfo)
        {
            var clientId = result.CustomerId;
            var checkId = result.ResultId;

            await _customerChecksRepository.AddAsync(new CustomerChecksInfo
            {
                CustomerId = clientId,

                LatestPepCheckId = checkId,
                LatestCrimeCheckId = checkId,
                LatestSanctionCheckId = checkId,

                IsPepCheckRequired = checksInfo.pep,
                IsCrimeCheckRequired = checksInfo.crime,
                IsSanctionCheckRequired = checksInfo.sanction
            });
        }

        private async Task<(bool pep, bool crime, bool sanction)> SaveDocuments(ISpiderCheckResult result)
        {
            var pepDiff = _diffService.ComputeDiffWithEmptyByPep(result);
            var crimeDiff = _diffService.ComputeDiffWithEmptyByCrime(result);
            var sanctionDiff = _diffService.ComputeDiffWithEmptyBySanction(result);

            var clientId = result.CustomerId;
            var checkId = result.ResultId;

            var pepTask = SaveDocument(clientId, pepDiff, PepSpiderCheck.ApiType, checkId);
            var crimeTask = SaveDocument(clientId, crimeDiff, CrimeSpiderCheck.ApiType, checkId);
            var sanctionTask = SaveDocument(clientId, sanctionDiff, SanctionSpiderCheck.ApiType, checkId);

            return (await pepTask, await crimeTask, await sanctionTask);
        }

        private async Task<bool> SaveDocument(string clientId, ISpiderCheckResultDiff diff, string type, string checkId)
        {
            var isAutoApprovedDiff = IsAutoApprovedDiff(diff);

            if (isAutoApprovedDiff)
            {
                var document = await _spiderCheckProcessingService.AddApprovedSpiderCheck(clientId, type,
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

                await SaveSpiderDocumentInfo(clientId, document.DocumentId, diff, checkId);
            }
            else
            {
                var document = await _spiderCheckProcessingService.UploadSpiderCheck(clientId, type, new KycChangeRequest
                {
                    Changer = SpiderChanger,
                    StatusComment = UploadedStatusComment
                });

                await SaveSpiderDocumentInfo(clientId, document.DocumentId, diff, checkId);
            }

            return isAutoApprovedDiff;
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
