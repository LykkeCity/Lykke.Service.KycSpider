namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderDocumentAutoStatusGroup
    {
        public SpiderDocumentAutoStatus Pep { get; set; }

        public SpiderDocumentAutoStatus Crime { get; set; }

        public SpiderDocumentAutoStatus Sanction { get; set; }
    }
}
