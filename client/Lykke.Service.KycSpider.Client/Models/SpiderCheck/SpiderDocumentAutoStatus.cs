namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderDocumentAutoStatus
    {
        public string DocumentId { get; set; }

        public string ApiType { get; set; }

        public bool IsAutoApproved { get; set; }
    }
}
