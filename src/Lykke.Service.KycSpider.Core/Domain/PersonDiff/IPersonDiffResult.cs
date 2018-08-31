using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Domain.PersonDiff
{
    public interface IPersonDiffResult
    {
        ISpiderCheckResultDiff PepDiff { get; }
        ISpiderCheckResultDiff CrimeDiff { get; }
        ISpiderCheckResultDiff SanctionDiff { get; }
    }
}
