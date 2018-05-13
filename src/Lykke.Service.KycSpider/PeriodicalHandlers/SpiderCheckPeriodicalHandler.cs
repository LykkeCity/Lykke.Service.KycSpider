using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.PeriodicalHandlers
{
    public class SpiderCheckPeriodicalHandler : TimerPeriod
    {
        private readonly ISpiderRegularCheckService _spiderRegularCheckService;

        public SpiderCheckPeriodicalHandler(ISpiderRegularCheckService spiderRegularCheckService, SpiderCheckSettings settings, ILog log) :
            base(nameof(SpiderCheckPeriodicalHandler), (int)settings.InstantCheckDelay.TotalMilliseconds, log)
        {
            _spiderRegularCheckService = spiderRegularCheckService;
        }

        public override async Task Execute()
        {
            await _spiderRegularCheckService.PerformCheckAsync();
        }
    }
}
