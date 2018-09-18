using System;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public class GlobalCheckInfo : IGlobalCheckInfo
    {
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }

        public int TotalClients { get; set; }
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
