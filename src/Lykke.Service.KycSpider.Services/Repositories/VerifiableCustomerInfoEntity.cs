using Lykke.AzureStorage.Tables;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class VerifiableCustomerInfoEntity : AzureTableEntity, IVerifiableCustomerInfo
    {
        public string CustomerId => RowKey;

        public string LatestSpiderCheckId { get; set; }
        public bool IsPepCheckRequired { get; set; }
        public bool IsCrimeCheckRequired { get; set; }
        public bool IsSanctionCheckRequired { get; set; }


        public static string GeneratePartitionKey(string clientId) => clientId.Substring(0, 4);
        public static string GenerateRowKey(string clientId) => clientId;

        public static VerifiableCustomerInfoEntity Create(IVerifiableCustomerInfo src)
        {
            return new VerifiableCustomerInfoEntity
            {
                PartitionKey = GeneratePartitionKey(src.CustomerId),
                RowKey = GenerateRowKey(src.CustomerId),

                LatestSpiderCheckId = src.LatestSpiderCheckId,
                IsCrimeCheckRequired = src.IsCrimeCheckRequired,
                IsPepCheckRequired = src.IsPepCheckRequired,
                IsSanctionCheckRequired = src.IsSanctionCheckRequired
            };
        }
    }
}
