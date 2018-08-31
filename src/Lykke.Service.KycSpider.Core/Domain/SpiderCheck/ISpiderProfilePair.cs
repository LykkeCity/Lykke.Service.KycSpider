namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public interface ISpiderProfilePair
    {
        ISpiderProfile Previous { get; }
        ISpiderProfile Current { get; }
    }
}
