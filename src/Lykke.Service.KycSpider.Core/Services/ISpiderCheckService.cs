using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ISpiderCheckService
    {
        Task<ISpiderCheckResult> CheckAsync(string clientId);
    }
}
