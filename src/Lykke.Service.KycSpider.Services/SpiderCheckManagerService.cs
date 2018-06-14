using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderCheckManagerService : ISpiderCheckManagerService
    {
        private readonly RepeatableTask _instantCheckRepeatableTask;
        private readonly RepeatableTask _regularCheckRepeatableTask;

        private readonly TimeSpan _dailyCheckTimeUtc;
        private readonly IGlobalCheckInfoService _globalCheckInfoService;
        private readonly ILog _log;

        public SpiderCheckManagerService
        (
            SpiderCheckSettings settings,
            ISpiderInstantCheckService instantCheckService,
            ISpiderRegularCheckService regularCheckService,
            IGlobalCheckInfoService globalCheckInfoService,
            ILog log
        )
        {
            _dailyCheckTimeUtc = settings.DailyCheckTimeUtc;
            _globalCheckInfoService = globalCheckInfoService;
            _log = log;

            if (_dailyCheckTimeUtc < TimeSpan.FromHours(0) || TimeSpan.FromHours(24) <= _dailyCheckTimeUtc)
            {
                throw new ArgumentException(
                    $"Incorrect time of day at {nameof(settings)}.{nameof(settings.DailyCheckTimeUtc)}",
                    nameof(settings));
            }

            _instantCheckRepeatableTask = new RepeatableTask(
                instantCheckService.PerformInstantCheckAsync,
                settings.InstantCheckDurationToWarn,
                async ex => await _log.WriteFatalErrorAsync(nameof(SpiderCheckManagerService), "Instant check", ex),
                async startTime => await _log.WriteWarningAsync(nameof(SpiderCheckManagerService), "Instant check", $"Instant check lasts more then {settings.InstantCheckDurationToWarn}. It starts at {startTime}")
            );

            _regularCheckRepeatableTask = new RepeatableTask(
                regularCheckService.PerformRegularCheckAsync,
                settings.RegularCheckDurationToWarn,
                async ex => await _log.WriteFatalErrorAsync(nameof(SpiderCheckManagerService), "Regular check", ex),
                async startTime => await _log.WriteWarningAsync(nameof(SpiderCheckManagerService), "Regular check", $"Regular check lasts more then {settings.RegularCheckDurationToWarn}. It starts at {startTime}")
            );
        }

        public async Task OnTimerAsync()
        {
            if (await IsDailyCheckShouldBePerformedNow())
            {
                await _regularCheckRepeatableTask.TryRepeatAsync();
            }

            await _regularCheckRepeatableTask.UpdateStatusAsync();
            await _instantCheckRepeatableTask.TryRepeatAsync();
        }

        public async Task<bool> TryStartInstantCheckManuallyAsync()
        {
            var result = await _instantCheckRepeatableTask.TryRepeatAsync();

            if (result)
            {
                await _log.WriteInfoAsync(nameof(SpiderCheckManagerService), nameof(TryStartInstantCheckManuallyAsync), "Instant check started manually");
            }
            else
            {
                await _log.WriteInfoAsync(nameof(SpiderCheckManagerService), nameof(TryStartInstantCheckManuallyAsync), "Instant did not start manually due to previous task in progress");
            }

            return result;
        }

        public async Task<bool> TryStartRegularCheckManuallyAsync()
        {
            var result = await _regularCheckRepeatableTask.TryRepeatAsync();

            if (result)
            {
                await _log.WriteInfoAsync(nameof(SpiderCheckManagerService), nameof(TryStartRegularCheckManuallyAsync), "Regular check started manually");
            }
            else
            {
                await _log.WriteInfoAsync(nameof(SpiderCheckManagerService), nameof(TryStartRegularCheckManuallyAsync), "Regular did not start manually due to previous task in progress");
            }

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
