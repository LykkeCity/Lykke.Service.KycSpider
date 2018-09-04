using System.Collections.Generic;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public interface ISpiderCheckResultDiff
    {
        IReadOnlyCollection<ISpiderProfile> RemovedProfiles { get; }
        IReadOnlyCollection<ISpiderProfile> AddedProfiles { get; }
        IReadOnlyCollection<ISpiderProfilePair> ChangedProfiles { get; }
    }
}
