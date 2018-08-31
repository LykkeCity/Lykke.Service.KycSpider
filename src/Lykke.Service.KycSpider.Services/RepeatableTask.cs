using System;
using System.Threading.Tasks;

namespace Lykke.Service.KycSpider.Services
{
    public class RepeatableTask
    {
        private readonly object _syncRoot = new object();

        private readonly Func<Task> _repeatableAsyncAction;
        private readonly Func<Exception, Task> _exceptionAsyncAction;
        private readonly Func<DateTime, Task> _durationProblemAsyncAction;

        public TimeSpan PermittedDuration { get; }

        private bool _isDurationProblemDetected;
        private DateTime _startDateTime;
        private Task _task;

        public RepeatableTask
        (
            Func<Task> repeatableAsyncAction,
            TimeSpan duration,
            Func<Exception, Task> exceptionAsyncAction,
            Func<DateTime, Task> durationProblemAsyncAction
        )
        {
            PermittedDuration = duration;
            _repeatableAsyncAction = repeatableAsyncAction;
            _exceptionAsyncAction = exceptionAsyncAction;
            _durationProblemAsyncAction = durationProblemAsyncAction;
        }

        public async Task UpdateStatusAsync()
        {
            var task = null as Task;
            var startTime = null as DateTime?;

            lock (_syncRoot)
            {
                if (_task == null)
                {
                    return;
                }

                if (!_isDurationProblemDetected && _startDateTime.Add(PermittedDuration) < DateTime.UtcNow)
                {
                    startTime = _startDateTime;
                    _isDurationProblemDetected = true;
                }

                if (_task.IsCompleted)
                {
                    task = _task;
                    _task = null;
                }
            }

            if (startTime != null)
            {
                await _durationProblemAsyncAction.Invoke(startTime.Value);
            }

            if (task != null)
            {
                try
                {
                    await task;
                }
                catch (Exception e)
                {
                    _exceptionAsyncAction?.Invoke(e);
                }
            }
        }

        public async Task<bool> TryRepeatAsync()
        {
            await UpdateStatusAsync();

            lock (_syncRoot)
            {
                if (_task != null)
                {
                    return false;
                }

                _task = _repeatableAsyncAction.Invoke();
                _isDurationProblemDetected = false;
                _startDateTime = DateTime.UtcNow;
            }

            return true;
        }
    }
}
