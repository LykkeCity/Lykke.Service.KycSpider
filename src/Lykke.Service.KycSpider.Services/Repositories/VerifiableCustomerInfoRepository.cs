using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class VerifiableCustomerInfoRepository : IVerifiableCustomerInfoRepository
    {
        private readonly INoSQLTableStorage<VerifiableCustomerInfoEntity> _tableStorage;

        public VerifiableCustomerInfoRepository(INoSQLTableStorage<VerifiableCustomerInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IVerifiableCustomerInfo> AddAsync(IVerifiableCustomerInfo entity)
        {
            var newEntity = VerifiableCustomerInfoEntity.Create(entity);
            await _tableStorage.InsertAsync(newEntity);
            return newEntity;
        }

        public async Task<IVerifiableCustomerInfo> UpdateCheckStatesAsync(string clientId, bool? pep = null, bool? crime = null, bool? sanction = null)
        {
            var entity = VerifiableCustomerInfoEntity.Create(clientId, pep, crime, sanction);
            await _tableStorage.InsertOrMergeAsync(entity);
            return entity;
        }

        public async Task<IVerifiableCustomerInfo> UpdateSpiderCheckIdAsync(string clientId, string spiderCheckId)
        {
            var entity = VerifiableCustomerInfoEntity.Create(clientId, spiderCheckId);
            await _tableStorage.InsertOrMergeAsync(entity);
            return entity;
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

        public async Task<IEnumerable<IVerifiableCustomerInfo>> GetBatch(int count, string separatingClientId = null)
        {
            if (separatingClientId == null)
            {
                return await _tableStorage.WhereAsync(new TableQuery<VerifiableCustomerInfoEntity>().Take(count));
            }

            var partitionKey = VerifiableCustomerInfoEntity.GeneratePartitionKey(separatingClientId);
            var rowKey = VerifiableCustomerInfoEntity.GenerateRowKey(separatingClientId);

            var query = new TableQuery<VerifiableCustomerInfoEntity>()
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

        public async Task<IVerifiableCustomerInfo> DeleteAsync(string clientId)
        {
            var partitionKey = VerifiableCustomerInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = VerifiableCustomerInfoEntity.GenerateRowKey(clientId);

            return await _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
