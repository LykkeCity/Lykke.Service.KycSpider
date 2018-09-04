using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ICustomerChecksService
    {
        Task<ICustomerChecksInfo> GetAsync(string clientId);
        Task<ICustomerChecksInfo> EnableCheck(string clientId, string apiType);

        Task<ISpiderDocumentInfo> GetSpiderDocumentInfo(string clientId, string documentId);
    }
}
