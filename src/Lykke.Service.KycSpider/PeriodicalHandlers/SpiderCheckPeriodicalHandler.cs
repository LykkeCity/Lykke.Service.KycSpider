using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.PeriodicalHandlers
{
    public class SpiderCheckPeriodicalHandler : TimerPeriod
    {
        private readonly ISpiderCheckManagerService _spiderCheckManagerService;

        public SpiderCheckPeriodicalHandler(ISpiderCheckManagerService spiderCheckManagerService, SpiderCheckSettings settings, ILog log) :
            base(nameof(SpiderCheckPeriodicalHandler), (int)settings.InstantCheckDelay.TotalMilliseconds, log)
        {
            _spiderCheckManagerService = spiderCheckManagerService;
        }

        public override async Task Execute()
        {
            await _spiderCheckManagerService.OnTimerAsync();
        }
    }
}
