using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.PeriodicalHandlers
{
    public class SpiderCheckPeriodicalHandler : TimerPeriod
    {
        private readonly ISpiderTimerCheckService _spiderTimerCheckService;

        public SpiderCheckPeriodicalHandler(ISpiderTimerCheckService spiderTimerCheckService, SpiderCheckSettings settings, ILog log) :
            base(nameof(SpiderCheckPeriodicalHandler), (int)settings.InstantCheckDelay.TotalMilliseconds, log)
        {
            _spiderTimerCheckService = spiderTimerCheckService;
        }

        public override async Task Execute()
        {
            await _spiderTimerCheckService.PerformCheckAsync();
        }
    }
}
