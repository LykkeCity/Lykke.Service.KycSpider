using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;
using Lykke.Service.KycSpider.Core.Repositories;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class SpiderDocumentInfoRepository : ISpiderDocumentInfoRepository
    {
        private readonly INoSQLTableStorage<SpiderDocumentInfoEntity> _tableStorage;

        public SpiderDocumentInfoRepository(INoSQLTableStorage<SpiderDocumentInfoEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<ISpiderDocumentInfo> AddOrUpdateAsync(ISpiderDocumentInfo entity)
        {
            var newEntity = SpiderDocumentInfoEntity.Create(entity);
            await _tableStorage.InsertOrReplaceAsync(newEntity);
            return newEntity;
        }

        public async Task<ISpiderDocumentInfo> GetAsync(string clientId, string documentId)
        {
            var partitionKey = SpiderDocumentInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = SpiderDocumentInfoEntity.GenerateRowKey(documentId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }

        public async Task<IEnumerable<ISpiderDocumentInfo>> GetAllByClientAsync(string clientId)
        {
            var partitionKey = SpiderDocumentInfoEntity.GeneratePartitionKey(clientId);

            return await _tableStorage.GetDataAsync(partitionKey);
        }

        public async Task<ISpiderDocumentInfo> DeleteAsync(string clientId, string documentId)
        {
            var partitionKey = SpiderDocumentInfoEntity.GeneratePartitionKey(clientId);
            var rowKey = SpiderDocumentInfoEntity.GenerateRowKey(documentId);

            return await _tableStorage.DeleteAsync(partitionKey, rowKey);
        }
    }
}
