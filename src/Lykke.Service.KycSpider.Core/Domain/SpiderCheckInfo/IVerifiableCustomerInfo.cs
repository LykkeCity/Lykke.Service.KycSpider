namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public interface IVerifiableCustomerInfo
    {
        string CustomerId { get; }

        string LatestSpiderCheckId { get; }
        bool IsPepCheckRequired { get; }
        bool IsCrimeCheckRequired { get; }
        bool IsSanctionCheckRequired { get; }
    } 
}
