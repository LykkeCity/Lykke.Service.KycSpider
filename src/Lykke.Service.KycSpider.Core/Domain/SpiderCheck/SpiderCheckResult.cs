using System;
using System.Collections.Generic;

namespace Lykke.Service.KycSpider.Core.Domain.SpiderCheck
{
    public class SpiderCheckResult : ISpiderCheckResult
    {
        public string CustomerId { get; set; }
        public string ResultId { get; set; }
        public long VerificationId { get; set; }
        public DateTime CheckDateTime { get; set; }
        public IReadOnlyCollection<ISpiderProfile> PersonProfiles { get; set; }
    }
}
