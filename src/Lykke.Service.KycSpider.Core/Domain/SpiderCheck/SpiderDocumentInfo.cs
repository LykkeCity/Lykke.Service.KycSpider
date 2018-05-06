using Lykke.Service.KycSpider.Core.Domain.CheckPerson;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderDocumentInfo : ISpiderDocumentInfo
    {
        public string CustomerId { get; set; }
        public string DocumentId { get; set; }

        public string CurrentCheckId { get; set; }
        public string PreviousCheckId { get; set; }

        public ICheckPersonResultDiff CheckDiff { get; set; }
    }
}
