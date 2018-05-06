using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public class GlobalCheckPersonRequest : IGlobalCheckPersonRequest
    {
        public IVerifiableCustomerInfo Customer { get; set; }
        public IKycCheckPersonResult CurrentResult { get; set; }
        public IKycCheckPersonResult PreviousResult { get; set; }
    }
}
