namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderProfilePair : ISpiderProfilePair
    {
        public SpiderProfile Previous { get; set; }
        public SpiderProfile Current { get; set; }

        ISpiderProfile ISpiderProfilePair.Previous => Previous;
        ISpiderProfile ISpiderProfilePair.Current => Current;
    }
}
