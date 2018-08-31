using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo;

namespace Lykke.Service.KycSpider.Core.Domain.PersonDiff
{
    public class PersonDiffRequest : IPersonDiffRequest
    {
        public IVerifiableCustomerInfo Customer { get; set; }
        public ISpiderCheckResult CurrentResult { get; set; }
        public ISpiderCheckResult PreviousResult { get; set; }
    }
}
