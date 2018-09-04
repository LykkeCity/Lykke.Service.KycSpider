using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Repositories
{
    public interface ICustomerChecksInfoRepository
    {
        Task<ICustomerChecksInfo> AddAsync(ICustomerChecksInfo entity);
        Task<ICustomerChecksInfo> UpdateCheckStatesAsync(string clientId, bool? pep = null, bool? crime = null, bool? sanction = null);
        Task<ICustomerChecksInfo> UpdateCheckIdsAsync(string clientId, string pepCheckId = null, string crimeCheckId = null, string sanctionCheckId = null);
        Task<ICustomerChecksInfo> GetAsync(string clientId);
        Task<IEnumerable<ICustomerChecksInfo>> GetPartitionAsync(string partitionKey);
        Task<IEnumerable<ICustomerChecksInfo>> GetBatch(int count, string separatingClientId = null);
        Task<ICustomerChecksInfo> DeleteAsync(string clientId);
    }
}
