using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.KycSpider.Core.Services;
using Lykke.Service.KycSpider.Core.Settings;

namespace Lykke.Service.KycSpider.Services
{
    public class SpiderTimerCheckService : ISpiderTimerCheckService
    {
        private readonly TimeSpan _dailyCheckTimeUtc;

        private readonly ISpiderInstantCheckService _instantCheckService;
        private readonly ISpiderRegularCheckService _regularCheckService;
        private readonly IGlobalCheckInfoService _globalCheckInfoService;
        private readonly ILog _log;

        public SpiderTimerCheckService
        (
            SpiderCheckSettings settings,
            ISpiderInstantCheckService instantCheckService,
            ISpiderRegularCheckService regularCheckService,
            IGlobalCheckInfoService globalCheckInfoService,
            ILog log           
        )
        {
            _dailyCheckTimeUtc = settings.DailyCheckTimeUtc;
            _instantCheckService = instantCheckService;
            _regularCheckService = regularCheckService;
            _globalCheckInfoService = globalCheckInfoService;
            _log = log;

            if (_dailyCheckTimeUtc < TimeSpan.FromHours(0) || TimeSpan.FromHours(24) <= _dailyCheckTimeUtc)
            {
                throw new ArgumentException($"Incorrect time of day at {nameof(settings)}.{nameof(settings.DailyCheckTimeUtc)}", nameof(settings));
            }
        }

        private Task _regularCheckTask;
        private DateTime _regularCheckStartDateTime;
        private const int RegularCheckHoursDurationToWarn = 20;
        private bool _regularCheckWarningLogged;

        public async Task PerformCheckAsync()
        {
            if (await IsDailyCheckShouldBePerformedNow())
            {
                if (_regularCheckTask == null)
                {
                    _regularCheckTask = _regularCheckService.PerformRegularCheckAsync();
                    _regularCheckStartDateTime = DateTime.UtcNow;
                    _regularCheckWarningLogged = false;
                }
                else if (_regularCheckTask.IsCompleted)
                {
                    var task = _regularCheckTask;
                    _regularCheckTask = null;
                    await task;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    if (_regularCheckStartDateTime.AddHours(RegularCheckHoursDurationToWarn) < now && !_regularCheckWarningLogged)
                    {
                        await _log.WriteWarningAsync(nameof(SpiderTimerCheckService), nameof(PerformCheckAsync),
                            $"Regular check lasts more then {RegularCheckHoursDurationToWarn} hours is starts at {_regularCheckStartDateTime}");
                        _regularCheckWarningLogged = true;
                    }
                }
            }

            await _instantCheckService.PerformInstantCheckAsync();
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
