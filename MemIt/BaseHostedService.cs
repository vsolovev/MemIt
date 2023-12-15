using Cronos;

namespace MemIt
{
    public class DefaultTimeProvider : ITimeProvider
    {
        public DateTime Now => DateTime.Now;
        public DateTime UtcNow => DateTime.UtcNow;
    }
    public interface ITimeProvider
    {
        DateTime Now { get; }
        DateTime UtcNow { get; }
    }
    public abstract class ScheduledBackgroundServiceBase : BackgroundService
    {
        private readonly ILogger _logger;
        protected readonly ITimeProvider _timeProvider;
        private readonly bool _triggerOnStart;
        private readonly bool _logWaiting;
        protected readonly CronExpression _schedule;
        protected readonly string _serviceName;

        protected ScheduledBackgroundServiceBase(ILogger logger, ITimeProvider timeProvider,
            string cronExpression, CronFormat cronFormat = CronFormat.Standard, bool triggerOnStart = false, bool logWaiting = false)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            _triggerOnStart = triggerOnStart;
            _logWaiting = logWaiting;
            _schedule = CronExpression.Parse(cronExpression, cronFormat);
            _serviceName = GetType().Name;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"{_serviceName}: started");
            try
            {
                if (_triggerOnStart)
                {
                    try
                    {
                        await Handle(_timeProvider.UtcNow, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, ex.Message);
                    }
                }

                while (!cancellationToken.IsCancellationRequested)
                {

                    try
                    {

                        var utcNow = _timeProvider.UtcNow;
                        var nextOccurence = _schedule.GetNextOccurrence(utcNow, true);
                        if (!nextOccurence.HasValue)
                        {
                            throw new Exception($"{_serviceName}: no next occurence scheduled, exiting");
                        }

                        if (_logWaiting)
                        {
                            _logger.LogDebug($"{_serviceName}: Waiting for the next occurence at '{nextOccurence.Value}'...");
                        }

                        var millisecondsToWait = Convert.ToInt32((nextOccurence.Value - utcNow).TotalMilliseconds);
                        await Task.Delay(millisecondsToWait, cancellationToken);

                        await Handle(nextOccurence.Value, cancellationToken);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"{_serviceName}: {ex.Message}");
                    }
                    finally
                    {
                    }
                }
            }
            finally
            {
                _logger.LogInformation($"{_serviceName}: stopped");
            }
        }

        protected abstract Task Handle(DateTime occurenceTimeUtc, CancellationToken cancellationToken);
    }
}
