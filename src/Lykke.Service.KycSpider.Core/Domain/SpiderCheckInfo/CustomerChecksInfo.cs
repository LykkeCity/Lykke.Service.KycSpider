namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public class CustomerChecksInfo : ICustomerChecksInfo
    {
        public string CustomerId { get; set; }
        public string LatestPepCheckId { get; set; }
        public string LatestCrimeCheckId { get; set; }
        public string LatestSanctionCheckId { get; set; }
        public bool IsPepCheckRequired { get; set; }
        public bool IsCrimeCheckRequired { get; set; }
        public bool IsSanctionCheckRequired { get; set; }
    }
}
