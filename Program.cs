using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using OnlinePlayersPool;
using OnlinePlayersPool.Notifications;
using OnlinePlayersPool.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>())
    .AddSingleton<Handler>()
    .AddSingleton<ILoggingSwitch, LoggingSwitch>()
    .AddSingleton<IPlayerPool, PlayerPool>()
    .AddSingleton<IEventQueue, EventQueue>()
    .AddSingleton<IMessageSender, MessageSender>()
    .AddHostedService<EventQueueRunner>()
    .AddHostedService<MessageSenderJob>()
    .AddHostedService<MetricsLoggerJob>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

var currentPlayerId = 0L;
var bdRankVsPlayerId = new ConcurrentDictionary<long, long>();
static long RandomRank() => Random.Shared.NextInt64(1000000);

app.MapPost("/setEnableLogging",
    (bool value, [FromServices] ILoggingSwitch loggingSwitch) =>
    {
        loggingSwitch.IsLoggingEnabled = value;
    });

app.MapPost("/addPlayer",
    ([Range(1, 10000)] int count, [FromServices] IEventQueue eventQueue, [FromServices] ILoggingSwitch loggingSwitch) =>
    {
        for (var i = count; i > 0; i--)
        {
            var playerId = Interlocked.Increment(ref currentPlayerId);
            var rank = bdRankVsPlayerId[playerId] = bdRankVsPlayerId.GetOrAdd(playerId, _ => RandomRank());
            eventQueue.Enqueue(new PlayerAdded(playerId, rank, loggingSwitch.IsLoggingEnabled));
        }
    });

app.MapPost("/removePlayer",
    (long id, [FromServices] IEventQueue eventQueue) =>
        eventQueue.Enqueue(new PlayerRemoved(id, true)));

app.MapPost("/changeRank",
    (long id, long oldRank, long newRank, [FromServices] IEventQueue eventQueue) =>
    {
        bdRankVsPlayerId[id] = newRank;
        eventQueue.Enqueue(new PlayerRankChanged(id, oldRank, newRank, true));
    });

app.MapPost("/stressTest",
    ([Range(1, int.MaxValue)] int count, [Range(1, 1000000)] int rankChangeExtent, [FromServices] IEventQueue eventQueue) =>
    {
        var maxPlayerId = currentPlayerId + 1;
        for (var i = count; i > 0; i--)
        {
            var playerId = Random.Shared.NextInt64(maxPlayerId);
            var oldRank = bdRankVsPlayerId.GetOrAdd(playerId, _ => RandomRank());
            var newRank = oldRank + Random.Shared.NextInt64(rankChangeExtent + rankChangeExtent + 1) - rankChangeExtent;

            eventQueue.Enqueue(new PlayerRankChanged(playerId, oldRank, newRank, false));
        }
    }
);

await app.RunAsync();
