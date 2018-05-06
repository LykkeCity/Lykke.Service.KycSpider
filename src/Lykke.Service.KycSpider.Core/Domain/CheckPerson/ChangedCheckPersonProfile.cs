using Lykke.Service.Kyc.Abstractions.Domain.Verification;

namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public class ChangedCheckPersonProfile : IChangedCheckPersonProfile
    {
        public IKycCheckPersonProfile Previous { get; set; }
        public IKycCheckPersonProfile Current { get; set; }
    }
}
