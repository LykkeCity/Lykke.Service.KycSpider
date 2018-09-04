using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Domain.PersonDiff
{
    public interface IPersonDiffRequest
    {
        ICustomerChecksInfo CustomerChecks { get; }

        ISpiderCheckResult CurrentResult { get; }
        ISpiderCheckResult PreviousResult { get; }
    }
}
