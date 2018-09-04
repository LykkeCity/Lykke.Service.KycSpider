using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Domain.PersonDiff
{
    public class PersonDiffRequest : IPersonDiffRequest
    {
        public ICustomerChecksInfo CustomerChecks { get; set; }
        public ISpiderCheckResult CurrentResult { get; set; }
        public ISpiderCheckResult PreviousResult { get; set; }
    }
}
