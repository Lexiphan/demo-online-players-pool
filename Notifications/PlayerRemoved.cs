using MediatR;

namespace OnlinePlayersPool.Notifications;

public sealed record PlayerRemoved(long Id, bool DoLog) : INotification;
