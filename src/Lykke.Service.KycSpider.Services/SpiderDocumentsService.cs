using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Lykke.Service.KycSpider.Core.Services;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderDocumentsService : ISpiderDocumentsService
    {
        private readonly ISpiderDocumentInfoRepository _repository;

        public SpiderDocumentsService(ISpiderDocumentInfoRepository repository)
        {
            _repository = repository;
        }

        public Task<ISpiderDocumentInfo> GetSpiderDocumentInfoAsync(string clientId, string documentId)
        {
            return _repository.GetAsync(clientId, documentId);
        }
    }
}
