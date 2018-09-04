using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderCheckManagerService : ISpiderCheckManagerService
    {
        private readonly RepeatableTask _regularCheckRepeatableTask;

        private readonly TimeSpan _dailyCheckTimeUtc;
        private readonly IGlobalCheckInfoService _globalCheckInfoService;
        private readonly ILog _log;

        public SpiderCheckManagerService
        (
            SpiderCheckSettings settings,
            ISpiderRegularCheckService regularCheckService,
            IGlobalCheckInfoService globalCheckInfoService,
            ILogFactory logFactory
        )
        {
            _dailyCheckTimeUtc = settings.DailyCheckTimeUtc;
            _globalCheckInfoService = globalCheckInfoService;
            _log = logFactory.CreateLog(this);

            if (_dailyCheckTimeUtc < TimeSpan.FromHours(0) || TimeSpan.FromHours(24) <= _dailyCheckTimeUtc)
            {
                throw new ArgumentException(
                    $"Incorrect time of day at {nameof(settings)}.{nameof(settings.DailyCheckTimeUtc)}",
                    nameof(settings));
            }

            _regularCheckRepeatableTask = new RepeatableTask(
                regularCheckService.PerformRegularCheckAsync,
                settings.RegularCheckDurationToWarn,
                async ex => _log.Critical("Regular check", ex),
                async startTime =>_log.Warning("Regular check", $"Regular check lasts more then {settings.RegularCheckDurationToWarn}. It starts at {startTime}")
            );
        }

        public async Task OnTimerAsync()
        {
            if (await IsDailyCheckShouldBePerformedNow())
            {
                await _regularCheckRepeatableTask.TryRepeatAsync();
            }

            await _regularCheckRepeatableTask.UpdateStatusAsync();
        }

        public async Task<bool> TryStartRegularCheckManuallyAsync()
        {
            var result = await _regularCheckRepeatableTask.TryRepeatAsync();

            _log.Info(result
                ? "Regular check started manually"
                : "Regular did not start manually due to previous task in progress");

            return result;
        }

        private async Task<bool> IsDailyCheckShouldBePerformedNow()
        {
            var now = DateTime.UtcNow;

            if (now.TimeOfDay <= _dailyCheckTimeUtc)
            {
                return false;
            }

            var latestCheckTimestamp = await _globalCheckInfoService.GetLatestCheckTimestamp();

            return !latestCheckTimestamp.HasValue || latestCheckTimestamp.Value < now.Date;
        }
    }
}
