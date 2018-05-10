using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Repositories
{
    public interface ISpiderCheckResultRepository
    {
        Task<ISpiderCheckResult> AddAsync(ISpiderCheckResult entity);
        Task<ISpiderCheckResult> GetAsync(string clientId, string resultId);
    }
}
