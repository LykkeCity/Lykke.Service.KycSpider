using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Repositories
{
    public interface ISpiderDocumentInfoRepository
    {
        Task<ISpiderDocumentInfo> AddOrUpdateAsync(ISpiderDocumentInfo entity);
        Task<ISpiderDocumentInfo> GetAsync(string clientId, string documentId);
        Task<IEnumerable<ISpiderDocumentInfo>> GetAllByClientAsync(string clientId);
        Task<ISpiderDocumentInfo> DeleteAsync(string clientId, string documentId);
    }
}
