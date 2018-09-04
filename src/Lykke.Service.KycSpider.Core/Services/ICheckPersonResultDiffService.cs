using Lykke.Service.KycSpider.Core.Domain.PersonDiff;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Services
{
    public interface ICheckPersonResultDiffService
    {
        IPersonDiffResult ComputeAllDiffs(IPersonDiffRequest request);

        ISpiderCheckResultDiff ComputeDiffByPep(ISpiderCheckResult current, ISpiderCheckResult previous);
        ISpiderCheckResultDiff ComputeDiffByCrime(ISpiderCheckResult current, ISpiderCheckResult previous);
        ISpiderCheckResultDiff ComputeDiffBySanction(ISpiderCheckResult current, ISpiderCheckResult previous);

        ISpiderCheckResultDiff ComputeDiffWithEmptyByPep(ISpiderCheckResult result);
        ISpiderCheckResultDiff ComputeDiffWithEmptyByCrime(ISpiderCheckResult result);
        ISpiderCheckResultDiff ComputeDiffWithEmptyBySanction(ISpiderCheckResult result);
    }
}
