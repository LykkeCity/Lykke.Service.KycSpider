using Lykke.Service.KycSpider.Core.Domain.CheckPerson;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public interface ISpiderDocumentInfo
    {
        string CustomerId { get; }
        string DocumentId { get; }

        string CurrentCheckId { get; }
        string PreviousCheckId { get; }

        ICheckPersonResultDiff CheckDiff { get; }
    } 
}
