using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Repositories
{
    public interface IVerifiableCustomerInfoRepository
    {
        Task<IVerifiableCustomerInfo> AddOrUpdateAsync(IVerifiableCustomerInfo entity);
        Task<IVerifiableCustomerInfo> GetAsync(string clientId);
        Task<IEnumerable<IVerifiableCustomerInfo>> GetPartitionAsync(string partitionKey);
        Task<IEnumerable<IVerifiableCustomerInfo>> GetBatch(int count, string separatingClientId = null);
        Task<IVerifiableCustomerInfo> DeleteAsync(string clientId);
    }
}
