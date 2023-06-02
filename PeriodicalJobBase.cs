using System.Diagnostics;

namespace OnlinePlayersPool;

internal abstract class PeriodicalJobBase : BackgroundService
{
    private readonly TimeSpan _minIntervalBetweenIterations;
    protected readonly ILogger Logger;

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var stopwatch = new Stopwatch();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                stopwatch.Restart();
                await IterationAsync(stoppingToken);

                var timeToWaitBeforeNextIteration = _minIntervalBetweenIterations - stopwatch.Elapsed;
                if (timeToWaitBeforeNextIteration > TimeSpan.Zero)
                {
                    await Task.Delay(timeToWaitBeforeNextIteration, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Unexpected exception");
            }
            stopwatch.Restart();
        }
    }

    protected abstract ValueTask IterationAsync(CancellationToken stoppingToken);

    protected PeriodicalJobBase(TimeSpan minIntervalBetweenIterations, ILogger logger)
    {
        _minIntervalBetweenIterations = minIntervalBetweenIterations;
        Logger = logger;
    }
}
