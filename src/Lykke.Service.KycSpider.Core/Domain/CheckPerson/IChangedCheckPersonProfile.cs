using Lykke.Service.Kyc.Abstractions.Domain.Verification;

namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public interface IChangedCheckPersonProfile
    {
        IKycCheckPersonProfile Previous { get; }
        IKycCheckPersonProfile Current { get; }
    }
}
