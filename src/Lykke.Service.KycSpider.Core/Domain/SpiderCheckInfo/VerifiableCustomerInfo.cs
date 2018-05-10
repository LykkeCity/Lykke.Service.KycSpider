namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public class VerifiableCustomerInfo : IVerifiableCustomerInfo
    {
        public string CustomerId { get; set; }
        public string LatestSpiderCheckId { get; set; }
        public bool IsPepCheckRequired { get; set; }
        public bool IsCrimeCheckRequired { get; set; }
        public bool IsSanctionCheckRequired { get; set; }
    }
}
