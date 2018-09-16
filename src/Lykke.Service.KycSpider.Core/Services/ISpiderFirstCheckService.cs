using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderFirstCheckService
    {
        Task<SpiderDocumentAutoStatusGroup> PerformFirstCheckAsync(string clientId, ISpiderCheckResult spiderResult = null);
    }
}
