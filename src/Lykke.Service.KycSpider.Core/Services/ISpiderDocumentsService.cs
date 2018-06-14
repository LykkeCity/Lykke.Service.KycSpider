using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderDocumentsService
    {
        Task<ISpiderDocumentInfo> GetSpiderDocumentInfoAsync(string clientId, string documentId);
    }
}
