using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class SpiderDocumentInfoEntity : AzureTableEntity, ISpiderDocumentInfo
    {
        public string CustomerId => PartitionKey;
        public string DocumentId => RowKey;

        public string CurrentCheckId { get; set; }
        public string PreviousCheckId { get; set; }

        [JsonValueSerializer]
        public ISpiderCheckResultDiff CheckDiff { get; set; }

        public static string GeneratePartitionKey(string documentId) => documentId;
        public static string GenerateRowKey(string clientId) => clientId;

        public static SpiderDocumentInfoEntity Create(ISpiderDocumentInfo src)
        {
            return new SpiderDocumentInfoEntity
            {
                PartitionKey = GeneratePartitionKey(src.CustomerId),
                RowKey = GenerateRowKey(src.DocumentId),

                CurrentCheckId = src.CurrentCheckId,
                PreviousCheckId = src.PreviousCheckId,
                CheckDiff = src.CheckDiff
            };
        }
    }
}
