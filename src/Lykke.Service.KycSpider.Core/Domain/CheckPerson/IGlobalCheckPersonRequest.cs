using Lykke.Service.Kyc.Abstractions.Domain.Verification;
using Lykke.Service.KycSpider.Core.Domain.SpiderCheck;

namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public interface IGlobalCheckPersonRequest
    {
        IVerifiableCustomerInfo Customer { get; }

        IKycCheckPersonResult CurrentResult { get; }
        IKycCheckPersonResult PreviousResult { get; }
    }
}
