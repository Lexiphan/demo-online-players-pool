using MediatR;
using OnlinePlayersPool.Services;

namespace OnlinePlayersPool;

internal class EventQueueRunner : BackgroundService
{
    private readonly IEventQueue _eventQueue;
    private readonly IMediator _mediator;
    private readonly ILogger<EventQueueRunner> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _eventQueue.WaitForNonEmptyAsync(stoppingToken))
        {
            try
            {
                var count = 0;
                while (_eventQueue.TryDequeue(out var evt))
                {
                    count++;
                    await _mediator.Publish(evt, stoppingToken);
                }

                if (count > 0)
                {
                    _logger.LogInformation("Published {Count} events", count);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unexpected exception while processing events");
            }
        }
    }

    public EventQueueRunner(IEventQueue eventQueue, IMediator mediator, ILogger<EventQueueRunner> logger)
    {
        _eventQueue = eventQueue;
        _mediator = mediator;
        _logger = logger;
    }
}
