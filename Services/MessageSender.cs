using OnlinePlayersPool.Models;

namespace OnlinePlayersPool.Services;

public interface IMessageSender
{
    Task SendAsync(LeaderboardMessage message, CancellationToken ct = default);
}

internal class MessageSender : IMessageSender
{
    private readonly ILogger<MessageSender> _logger;
    private readonly ILoggingSwitch _loggingSwitch;

    public async Task SendAsync(LeaderboardMessage message, CancellationToken ct = default)
    {
        await Task.Yield();
        // await Task.Delay(50, ct); // simulate some SignalR delay
        if (_loggingSwitch.IsLoggingEnabled)
        {
            _logger.LogInformation("Player {PlayerId} new rank {PlayerRank}", message.PlayerId, message.Rank);
        }
    }

    public MessageSender(ILogger<MessageSender> logger, ILoggingSwitch loggingSwitch)
    {
        _logger = logger;
        _loggingSwitch = loggingSwitch;
    }
}
