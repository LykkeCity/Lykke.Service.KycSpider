using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Repositories
{
    public interface IVerifiableCustomerInfoRepository
    {
        Task<IVerifiableCustomerInfo> AddOrUpdateAsync(IVerifiableCustomerInfo entity);
        Task<IVerifiableCustomerInfo> GetAsync(string clientId);
        Task<IEnumerable<IVerifiableCustomerInfo>> GetPartitionAsync(string partitionKey);
        Task<IVerifiableCustomerInfo> DeleteAsync(string clientId);
    }
}
