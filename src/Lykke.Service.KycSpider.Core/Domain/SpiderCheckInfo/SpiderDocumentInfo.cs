using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public class SpiderDocumentInfo : ISpiderDocumentInfo
    {
        public string CustomerId { get; set; }
        public string DocumentId { get; set; }

        public string CurrentCheckId { get; set; }
        public string PreviousCheckId { get; set; }

        public SpiderCheckResultDiff CheckDiff { get; set; }

        ISpiderCheckResultDiff ISpiderDocumentInfo.CheckDiff => CheckDiff;
    }
}
