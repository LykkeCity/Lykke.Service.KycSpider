using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Repositories;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class VerifiableCustomerInfoRepository : IVerifiableCustomerInfoRepository
    {
        private readonly INoSQLTableStorage<VerifiableCustomerInfoEntity> _tableStorage;

        public VerifiableCustomerInfoRepository(INoSQLTableStorage<VerifiableCustomerInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IVerifiableCustomerInfo> AddOrUpdateAsync(IVerifiableCustomerInfo entity)
        {
            var newEntity = VerifiableCustomerInfoEntity.Create(entity);
            await _tableStorage.InsertOrReplaceAsync(newEntity);
            return newEntity;
        }

        public async Task<IVerifiableCustomerInfo> GetAsync(string clientId)
        {
            var partitionKey = VerifiableCustomerInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = VerifiableCustomerInfoEntity.GenerateRowKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IVerifiableCustomerInfo>> GetPartitionAsync(string partitionKey)
        {
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IVerifiableCustomerInfo> DeleteAsync(string clientId)
        {
            var partitionKey = VerifiableCustomerInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = VerifiableCustomerInfoEntity.GenerateRowKey(clientId);

            return await _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
