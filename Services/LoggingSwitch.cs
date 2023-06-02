namespace OnlinePlayersPool.Services;

public interface ILoggingSwitch
{
    bool IsLoggingEnabled { get; set; }
}

internal class LoggingSwitch : ILoggingSwitch
{
    public bool IsLoggingEnabled { get; set; }
}
