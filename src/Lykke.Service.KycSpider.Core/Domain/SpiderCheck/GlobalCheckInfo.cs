using System;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class GlobalCheckInfo : IGlobalCheckInfo
    {
        public DateTimeOffset Timestamp { get; set; }

        public int SpiderChecks { get; set; }
        public int PepSuspects { get; set; }
        public int CrimeSuspects { get; set; }
        public int SanctionSuspects { get; set; }
        public int TotalProfiles { get; set; }
        public int AddedProfiles { get; set; }
        public int RemovedProfiles { get; set; }
        public int ChangedProfiles { get; set; }
    }
}
