using System;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheckInfo
{
    public interface IGlobalCheckInfo
    {
        DateTime StartDateTime { get; }
        DateTime EndDateTime { get; }

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
