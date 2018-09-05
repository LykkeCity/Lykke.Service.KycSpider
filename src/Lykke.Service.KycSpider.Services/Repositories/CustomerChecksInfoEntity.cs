using Lykke.AzureStorage.Tables;
using Lykke.AzureStorage.Tables.Entity.Annotation;
using Lykke.AzureStorage.Tables.Entity.ValueTypesMerging;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    [ValueTypeMergingStrategy(ValueTypeMergingStrategy.UpdateIfDirty)]
    public class CustomerChecksInfoEntity : AzureTableEntity, ICustomerChecksInfo
    {
        public string CustomerId => RowKey;

        public string LatestPepCheckId { get; set; }
        public string LatestCrimeCheckId { get; set; }
        public string LatestSanctionCheckId { get; set; }

        public bool IsPepCheckRequired { get; set; }
        public bool IsCrimeCheckRequired { get; set; }
        public bool IsSanctionCheckRequired { get; set; }

        public static string GeneratePartitionKey(string clientId) => clientId.Substring(0, 4);
        public static string GenerateRowKey(string clientId) => clientId;

        public static CustomerChecksInfoEntity Create(ICustomerChecksInfo src)
        {
            var entity = new CustomerChecksInfoEntity
            {
                PartitionKey = GeneratePartitionKey(src.CustomerId),
                RowKey = GenerateRowKey(src.CustomerId),

                LatestPepCheckId = src.LatestPepCheckId,
                LatestCrimeCheckId = src.LatestCrimeCheckId,
                LatestSanctionCheckId = src.LatestSanctionCheckId,

                IsPepCheckRequired = src.IsPepCheckRequired,
                IsCrimeCheckRequired = src.IsCrimeCheckRequired,
                IsSanctionCheckRequired = src.IsSanctionCheckRequired
            };

            entity.MarkValueTypePropertyAsDirty(nameof(IsPepCheckRequired));
            entity.MarkValueTypePropertyAsDirty(nameof(IsCrimeCheckRequired));
            entity.MarkValueTypePropertyAsDirty(nameof(IsSanctionCheckRequired));

            return entity;
        }

        public static CustomerChecksInfoEntity Create(string clientId, bool? pep, bool? crime, bool? sanction)
        {
            var entity = new CustomerChecksInfoEntity
            {
                PartitionKey = GeneratePartitionKey(clientId),
                RowKey = GenerateRowKey(clientId),
            };

            if (pep != null)
            {
                entity.IsPepCheckRequired = pep.Value;
                entity.MarkValueTypePropertyAsDirty(nameof(IsPepCheckRequired));
            }

            if (crime != null)
            {
                entity.IsCrimeCheckRequired = crime.Value;
                entity.MarkValueTypePropertyAsDirty(nameof(IsCrimeCheckRequired));
            }

            if (sanction != null)
            {
                entity.IsSanctionCheckRequired = sanction.Value;
                entity.MarkValueTypePropertyAsDirty(nameof(IsSanctionCheckRequired));
            }

            return entity;
        }

        public static CustomerChecksInfoEntity Create(string clientId, string pepCheckId, string crimeCheckId, string sanctionCheckId)
        {
            return new CustomerChecksInfoEntity
            {
                PartitionKey = GeneratePartitionKey(clientId),
                RowKey = GenerateRowKey(clientId),

                LatestPepCheckId = pepCheckId,
                LatestCrimeCheckId = crimeCheckId,
                LatestSanctionCheckId = sanctionCheckId,
            };
        }
    }
}
