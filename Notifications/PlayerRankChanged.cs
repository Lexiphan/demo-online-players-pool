using MediatR;

namespace OnlinePlayersPool.Notifications;

public sealed record PlayerRankChanged(long Id, long OldRank, long NewRank, bool DoLog) : INotification;
