using System.Collections.Generic;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;

namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public class CheckPersonResultDiff : ICheckPersonResultDiff
    {
        public IReadOnlyCollection<IKycCheckPersonProfile> RemovedProfiles { get; set; }
        public IReadOnlyCollection<IKycCheckPersonProfile> AddedProfiles { get; set; }
        public IReadOnlyCollection<IChangedCheckPersonProfile> ChangedProfiles { get; set; }
    }
}
