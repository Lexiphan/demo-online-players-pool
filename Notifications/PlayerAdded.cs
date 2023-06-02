using MediatR;

namespace OnlinePlayersPool.Notifications;

public sealed record PlayerAdded(long Id, long Rank, bool DoLog) : INotification;
