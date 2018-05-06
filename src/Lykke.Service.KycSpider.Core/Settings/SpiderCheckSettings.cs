using System;

namespace Lykke.Service.KycSpider.Core.Settings
{
    public class SpiderCheckSettings
    {
        public TimeSpan InstantCheckDelay { get; set; }
        public TimeSpan DailyCheckTimeUtc { get; set; }
    }
}
