using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class CustomerChecksInfoRepository : ICustomerChecksInfoRepository
    {
        private readonly INoSQLTableStorage<CustomerChecksInfoEntity> _tableStorage;

        public CustomerChecksInfoRepository(INoSQLTableStorage<CustomerChecksInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<ICustomerChecksInfo> AddAsync(ICustomerChecksInfo entity)
        {
            var newEntity = CustomerChecksInfoEntity.Create(entity);
            await _tableStorage.InsertAsync(newEntity);
            return newEntity;
        }

        public async Task<ICustomerChecksInfo> UpdateCheckStatesAsync(string clientId, bool? pep = null, bool? crime = null, bool? sanction = null)
        {
            var entity = CustomerChecksInfoEntity.Create(clientId, pep, crime, sanction);
            await _tableStorage.InsertOrMergeAsync(entity);
            return entity;
        }

        public async Task<ICustomerChecksInfo> UpdateCheckIdsAsync(string clientId, string pepCheckId = null, string crimeCheckId = null, string sanctionCheckId = null)
        {
            var entity = CustomerChecksInfoEntity.Create(clientId, pepCheckId, crimeCheckId, sanctionCheckId);
            await _tableStorage.InsertOrMergeAsync(entity);
            return entity;
        }

        public async Task<ICustomerChecksInfo> GetAsync(string clientId)
        {
            var partitionKey = CustomerChecksInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = CustomerChecksInfoEntity.GenerateRowKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<ICustomerChecksInfo>> GetPartitionAsync(string partitionKey)
        {
            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IEnumerable<ICustomerChecksInfo>> GetBatch(int count, string separatingClientId = null)
        {
            if (separatingClientId == null)
            {
                return await _tableStorage.WhereAsync(new TableQuery<CustomerChecksInfoEntity>().Take(count));
            }

            var partitionKey = CustomerChecksInfoEntity.GeneratePartitionKey(separatingClientId);
            var rowKey = CustomerChecksInfoEntity.GenerateRowKey(separatingClientId);

            var query = new TableQuery<CustomerChecksInfoEntity>()
                .Where(TableQuery.CombineFilters(
                    TableQuery.GenerateFilterCondition(
                        nameof(ITableEntity.PartitionKey),
                        QueryComparisons.GreaterThanOrEqual,
                        partitionKey),
                    TableOperators.And,
                    TableQuery.GenerateFilterCondition(
                        nameof(ITableEntity.RowKey),
                        QueryComparisons.GreaterThan,
                        rowKey)
                )).Take(count);

            return await _tableStorage.WhereAsync(query);
        }

        public async Task<ICustomerChecksInfo> DeleteAsync(string clientId)
        {
            var partitionKey = CustomerChecksInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = CustomerChecksInfoEntity.GenerateRowKey(clientId);

            return await _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
