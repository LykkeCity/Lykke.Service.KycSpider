using System.Collections.Generic;
using Lykke.Service.Kyc.Abstractions.Domain.Verification;

namespace Lykke.Service.KycSpider.Core.Domain.CheckPerson
{
    public interface ICheckPersonResultDiff
    {
        IReadOnlyCollection<IKycCheckPersonProfile> RemovedProfiles { get; }
        IReadOnlyCollection<IKycCheckPersonProfile> AddedProfiles { get; }
        IReadOnlyCollection<IChangedCheckPersonProfile> ChangedProfiles { get; }
    }
}
