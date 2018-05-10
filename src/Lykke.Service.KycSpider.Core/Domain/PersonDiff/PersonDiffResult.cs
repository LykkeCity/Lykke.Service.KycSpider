using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Domain.PersonDiff
{
    public class PersonDiffResult : IPersonDiffResult
    {
        public ISpiderCheckResultDiff PepDiff { get; set; }
        public ISpiderCheckResultDiff CrimeDiff { get; set; }
        public ISpiderCheckResultDiff SanctionDiff { get; set; }
    }
}
