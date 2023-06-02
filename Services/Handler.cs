using MediatR;
using OnlinePlayersPool.Models;
using OnlinePlayersPool.Notifications;

namespace OnlinePlayersPool.Services;

public sealed class Handler :
    INotificationHandler<PlayerAdded>,
    INotificationHandler<PlayerRemoved>,
    INotificationHandler<PlayerRankChanged>
{
    private readonly IPlayerPool _playerPool;
    private readonly ILogger<Handler> _logger;

    public Task Handle(PlayerAdded evt, CancellationToken ct)
    {
        _playerPool.AddPlayer(new Player(evt.Id, evt.Rank));
        if (evt.DoLog)
        {
            _logger.LogInformation("Event: Player {PlayerId} added, rank {PlayerRank}", evt.Id, evt.Rank);
        }
        return Task.CompletedTask;
    }

    public Task Handle(PlayerRemoved evt, CancellationToken ct)
    {
        if (_playerPool.RemovePlayer(evt.Id) && evt.DoLog)
        {
            _logger.LogInformation("Event: Player {PlayerId} removed", evt.Id);
        }
        return Task.CompletedTask;
    }

    public Task Handle(PlayerRankChanged evt, CancellationToken ct)
    {
        _playerPool.ChangePlayerRank(evt.Id, evt.OldRank, evt.NewRank);
        if (evt.DoLog)
        {
            _logger.LogInformation("Event: Player {PlayerId} changed rank {OldRank} -> {NewRank}", evt.Id, evt.OldRank, evt.NewRank);
        }
        return Task.CompletedTask;
    }

    public Handler(IPlayerPool playerPool, ILogger<Handler> logger)
    {
        _playerPool = playerPool;
        _logger = logger;
    }
}
