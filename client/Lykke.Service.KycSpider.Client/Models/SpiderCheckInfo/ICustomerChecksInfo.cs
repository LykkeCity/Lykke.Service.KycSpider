namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public interface ICustomerChecksInfo
    {
        string CustomerId { get; }

        string LatestPepCheckId { get; }
        string LatestCrimeCheckId { get; }
        string LatestSanctionCheckId { get; }
        bool IsPepCheckRequired { get; }
        bool IsCrimeCheckRequired { get; }
        bool IsSanctionCheckRequired { get; }
    } 
}
