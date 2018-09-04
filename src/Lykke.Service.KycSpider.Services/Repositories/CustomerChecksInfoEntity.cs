using Lykke.AzureStorage.Tables;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class CustomerChecksInfoEntity : AzureTableEntity, ICustomerChecksInfo
    {
        public string CustomerId => RowKey;

        public string LatestPepCheckId { get; set; }
        public string LatestCrimeCheckId { get; set; }
        public string LatestSanctionCheckId { get; set; }

        public bool? IsPepCheckRequired { get; set; }
        public bool? IsCrimeCheckRequired { get; set; }
        public bool? IsSanctionCheckRequired { get; set; }

        bool ICustomerChecksInfo.IsPepCheckRequired => IsPepCheckRequired == true;
        bool ICustomerChecksInfo.IsCrimeCheckRequired => IsCrimeCheckRequired == true;
        bool ICustomerChecksInfo.IsSanctionCheckRequired => IsSanctionCheckRequired == true;

        public static string GeneratePartitionKey(string clientId) => clientId.Substring(0, 4);
        public static string GenerateRowKey(string clientId) => clientId;

        public static CustomerChecksInfoEntity Create(ICustomerChecksInfo src)
        {
            return new CustomerChecksInfoEntity
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
        }

        public static CustomerChecksInfoEntity Create(string clientId, bool? pep, bool? crime, bool? sanction)
        {
            return new CustomerChecksInfoEntity
            {
                PartitionKey = GeneratePartitionKey(clientId),
                RowKey = GenerateRowKey(clientId),

                IsPepCheckRequired = pep,
                IsCrimeCheckRequired = crime,
                IsSanctionCheckRequired = sanction
            };
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
