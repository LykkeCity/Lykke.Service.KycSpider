using System.Collections.Generic;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderCheckResultDiff : ISpiderCheckResultDiff
    {
        public IReadOnlyCollection<SpiderProfile> RemovedProfiles { get; set; }
        public IReadOnlyCollection<SpiderProfile> AddedProfiles { get; set; }
        public IReadOnlyCollection<SpiderProfilePair> ChangedProfiles { get; set; }

        IReadOnlyCollection<ISpiderProfile> ISpiderCheckResultDiff.RemovedProfiles => RemovedProfiles;
        IReadOnlyCollection<ISpiderProfile> ISpiderCheckResultDiff.AddedProfiles => AddedProfiles;
        IReadOnlyCollection<ISpiderProfilePair> ISpiderCheckResultDiff.ChangedProfiles => ChangedProfiles;
    }
}
