using Dasync.Collections;
using OnlinePlayersPool.Services;

namespace OnlinePlayersPool;

internal class MessageSenderJob : PeriodicalJobBase
{
    private readonly IPlayerPool _playerPool;
    private readonly IMessageSender _messageSender;

    protected override async ValueTask IterationAsync(CancellationToken stoppingToken)
    {
        var processed = 0;

        await _playerPool.IterateThroughChanged().ParallelForEachAsync(
            player =>
            {
                Interlocked.Increment(ref processed);
                return _messageSender.SendAsync(player.GetMessage(), stoppingToken);
            },
            stoppingToken);

        if (processed > 0)
        {
            Logger.LogInformation("Processed {Count} players", processed);
        }
    }

    public MessageSenderJob(IPlayerPool playerPool, ILogger<MessageSenderJob> logger, IMessageSender messageSender)
        : base(TimeSpan.FromSeconds(1), logger)
    {
        _playerPool = playerPool;
        _messageSender = messageSender;
    }
}
