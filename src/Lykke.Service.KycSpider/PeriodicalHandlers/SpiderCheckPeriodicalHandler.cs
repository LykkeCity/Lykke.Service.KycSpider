using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.PeriodicalHandlers
{
    public class SpiderCheckPeriodicalHandler : TimerPeriod
    {
        private readonly ISpiderCheckManagerService _spiderCheckManagerService;

        public SpiderCheckPeriodicalHandler
        (
            ISpiderCheckManagerService spiderCheckManagerService,
            SpiderCheckSettings settings,
            ILogFactory logFactory
        ) : base(settings.InstantCheckDelay, logFactory)
        {
            _spiderCheckManagerService = spiderCheckManagerService;
        }

        public override async Task Execute()
        {
            await _spiderCheckManagerService.OnTimerAsync();
        }
    }
}
