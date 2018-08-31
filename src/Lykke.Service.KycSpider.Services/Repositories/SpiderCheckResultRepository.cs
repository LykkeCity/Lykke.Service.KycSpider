using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Repositories;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class SpiderCheckResultRepository : ISpiderCheckResultRepository
    {
        private readonly INoSQLTableStorage<SpiderCheckResultEntity> _tableStorage;

        public SpiderCheckResultRepository(INoSQLTableStorage<SpiderCheckResultEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<ISpiderCheckResult> AddAsync(ISpiderCheckResult entity)
        {
            var newEntity = SpiderCheckResultEntity.Create(entity);
            await _tableStorage.InsertOrReplaceAsync(newEntity);
            return newEntity;
        }

        public async Task<ISpiderCheckResult> GetAsync(string clientId, string resultId)
        {
            var partitionKey = SpiderCheckResultEntity.GeneratePartitionKey(clientId);
            var rowKey = SpiderCheckResultEntity.GenerateRowKey(resultId);

            return await _tableStorage.GetDataAsync(partitionKey, rowKey);
        }
    }
}
