using OnlinePlayersPool.Services;

namespace OnlinePlayersPool;

internal class MetricsLoggerJob : PeriodicalJobBase
{
    private readonly IPlayerPool _playerPool;
    private readonly IEventQueue _eventQueue;

    private int _previousOnlinePlayersCount;
    private int _previousQueueSize;

    protected override ValueTask IterationAsync(CancellationToken stoppingToken)
    {
        var onlinePlayersCount = _playerPool.OnlinePlayersCount;
        var queueSize = _eventQueue.QueueSize;

        if (_previousOnlinePlayersCount != onlinePlayersCount || _previousQueueSize != queueSize)
        {
            _previousOnlinePlayersCount = onlinePlayersCount;
            _previousQueueSize = queueSize;

            Logger.LogInformation(
                "Online = {OnlinePlayersCount}, QueueSize = {QueueSize}",
                onlinePlayersCount,
                queueSize);
        }

        return ValueTask.CompletedTask;
    }

    public MetricsLoggerJob(IPlayerPool playerPool, IEventQueue eventQueue, ILogger<MetricsLoggerJob> logger)
        : base(TimeSpan.FromSeconds(1), logger)
    {
        _playerPool = playerPool;
        _eventQueue = eventQueue;
    }
}
