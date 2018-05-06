using System;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public interface IGlobalCheckInfo
    {
        DateTimeOffset Timestamp { get; }

        int SpiderChecks { get; }
        int PepSuspects { get; }
        int CrimeSuspects { get; }
        int SanctionSuspects { get; }
        int TotalProfiles { get; }
        int AddedProfiles { get; }
        int RemovedProfiles { get; }
        int ChangedProfiles { get; } 
    } 
}
