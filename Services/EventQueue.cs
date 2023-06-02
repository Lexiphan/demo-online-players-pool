using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using MediatR;

namespace OnlinePlayersPool.Services;

public interface IEventQueue
{
    int QueueSize { get; }
    void Enqueue(INotification evt);
    Task<bool> WaitForNonEmptyAsync(CancellationToken ct);
    bool TryDequeue([MaybeNullWhen(false)] out INotification evt);
}

internal class EventQueue : IEventQueue
{
    private readonly ConcurrentQueue<INotification> _queue = new();
    private readonly AutoResetEvent _manualResetEvent = new(false);

    public int QueueSize => _queue.Count;

    public void Enqueue(INotification evt)
    {
        _queue.Enqueue(evt);
        _manualResetEvent.Set();
    }

    public Task<bool> WaitForNonEmptyAsync(CancellationToken ct) => _manualResetEvent.WaitOneAsync(ct);

    public bool TryDequeue([MaybeNullWhen(false)] out INotification evt) => _queue.TryDequeue(out evt);
}
