using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public interface ISpiderDocumentInfo
    {
        string CustomerId { get; }
        string DocumentId { get; }

        string CurrentCheckId { get; }
        string PreviousCheckId { get; }

        ISpiderCheckResultDiff CheckDiff { get; }
    } 
}
