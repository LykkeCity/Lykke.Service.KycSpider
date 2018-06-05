using Lykke.AzureStorage.Tables;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Services.Repositories
{
    public class VerifiableCustomerInfoEntity : AzureTableEntity, IVerifiableCustomerInfo
    {
        public string CustomerId => RowKey;

        public string LatestSpiderCheckId { get; set; }
        public bool? IsPepCheckRequired { get; set; }
        public bool? IsCrimeCheckRequired { get; set; }
        public bool? IsSanctionCheckRequired { get; set; }

        bool IVerifiableCustomerInfo.IsPepCheckRequired => IsPepCheckRequired == true;
        bool IVerifiableCustomerInfo.IsCrimeCheckRequired => IsCrimeCheckRequired == true;
        bool IVerifiableCustomerInfo.IsSanctionCheckRequired => IsSanctionCheckRequired == true;

        public static string GeneratePartitionKey(string clientId) => clientId.Substring(0, 4);
        public static string GenerateRowKey(string clientId) => clientId;

        public static VerifiableCustomerInfoEntity Create(IVerifiableCustomerInfo src)
        {
            return new VerifiableCustomerInfoEntity
            {
                PartitionKey = GeneratePartitionKey(src.CustomerId),
                RowKey = GenerateRowKey(src.CustomerId),

                LatestSpiderCheckId = src.LatestSpiderCheckId,
                IsPepCheckRequired = src.IsPepCheckRequired,
                IsCrimeCheckRequired = src.IsCrimeCheckRequired,
                IsSanctionCheckRequired = src.IsSanctionCheckRequired
            };
        }

        public static VerifiableCustomerInfoEntity Create(string clientId, bool? pep, bool? crime, bool? sanction)
        {
            return new VerifiableCustomerInfoEntity
            {
                PartitionKey = GeneratePartitionKey(clientId),
                RowKey = GenerateRowKey(clientId),

                IsPepCheckRequired = pep,
                IsCrimeCheckRequired = crime,
                IsSanctionCheckRequired = sanction
            };
        }

        public static VerifiableCustomerInfoEntity Create(string clientId, string spiderCheckId)
        {
            return new VerifiableCustomerInfoEntity
            {
                PartitionKey = GeneratePartitionKey(clientId),
                RowKey = GenerateRowKey(clientId),

                LatestSpiderCheckId = spiderCheckId
            };
        }
    }
}
