using System;
using System.Collections.Generic;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public interface ISpiderCheckResult
    {
        string CustomerId { get; }
        string ResultId { get; }

        long VerificationId { get; }
        DateTime CheckDateTime { get; }

        IReadOnlyCollection<ISpiderProfile> PersonProfiles { get; }
    }
}
