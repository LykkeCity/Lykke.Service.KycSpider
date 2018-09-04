using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class CustomerChecksService : ICustomerChecksService
    {
        private readonly ICustomerChecksInfoRepository _checksInfoRepository;
        private readonly ISpiderDocumentInfoRepository _documentInfoRepository;
        private readonly ISpiderFirstCheckService _firstCheckService;

        public CustomerChecksService
        (
            ICustomerChecksInfoRepository checksInfoRepository,
            ISpiderDocumentInfoRepository documentInfoRepository,
            ISpiderFirstCheckService firstCheckService

        )
        {
            _checksInfoRepository = checksInfoRepository;
            _documentInfoRepository = documentInfoRepository;
            _firstCheckService = firstCheckService;
        }


        public Task<ISpiderDocumentInfo> GetSpiderDocumentInfo(string clientId, string documentId)
        {
            return _documentInfoRepository.GetAsync(clientId, documentId);
        }

        public Task<ICustomerChecksInfo> GetAsync(string clientId)
        {
            return _checksInfoRepository.GetAsync(clientId);
        }

        public Task<ICustomerChecksInfo> EnableCheck(string clientId, string apiType)
        {
            return _checksInfoRepository.UpdateCheckStatesAsync(clientId, apiType, true);
        }
    }
}

