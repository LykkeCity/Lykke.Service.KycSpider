using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Repositories;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class GlobalCheckInfoRepository : IGlobalCheckInfoRepository
    {
        private readonly INoSQLTableStorage<GlobalCheckInfoEntity> _tableStorage;

        public GlobalCheckInfoRepository(INoSQLTableStorage<GlobalCheckInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IGlobalCheckInfo> AddOrUpdateAsync(IGlobalCheckInfo entity)
        {
            var newEntity = GlobalCheckInfoEntity.Create(entity);
            await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(newEntity, newEntity.Timestamp.DateTime);
            return newEntity;
        }

        public async Task<IGlobalCheckInfo> GetAsync(DateTimeOffset timestamp)
        {
            var partitionKey = GlobalCheckInfoEntity.GeneratePartitionKey(timestamp);
            var rowKey = GlobalCheckInfoEntity.GenerateRowKey(timestamp);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<IGlobalCheckInfo>> GetAllByYearAsync(int year)
        {
            var partitionKey = GlobalCheckInfoEntity.GeneratePartitionKey(year);

            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<IGlobalCheckInfo> DeleteAsync(DateTimeOffset timestamp)
        {
            var partitionKey = GlobalCheckInfoEntity.GeneratePartitionKey(timestamp);
            var rowKey = GlobalCheckInfoEntity.GenerateRowKey(timestamp);

            return await _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
