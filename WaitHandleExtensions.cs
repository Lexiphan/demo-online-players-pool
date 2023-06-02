namespace OnlinePlayersPool;

public static class WaitHandleExtensions
{
    public static async Task<bool> WaitOneAsync(this WaitHandle handle, int millisecondsTimeout, CancellationToken ct = default)
    {
        if (ct.IsCancellationRequested)
        {
            return false;
        }

        var registeredHandle = default(RegisteredWaitHandle);

        try
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            registeredHandle = ThreadPool.RegisterWaitForSingleObject(
                handle,
                (state, timedOut) => ((TaskCompletionSource<bool>)state!).TrySetResult(!timedOut),
                tcs,
                millisecondsTimeout,
                true);

            await using var tokenRegistration = ct.Register(
                state => ((TaskCompletionSource<bool>)state!).TrySetResult(false),
                tcs);

            return await tcs.Task;
        }
        finally
        {
            registeredHandle?.Unregister(null);
        }
    }

    public static Task<bool> WaitOneAsync(this WaitHandle handle, TimeSpan timeout, CancellationToken ct = default)
    {
        return handle.WaitOneAsync((int)timeout.TotalMilliseconds, ct);
    }

    public static Task<bool> WaitOneAsync(this WaitHandle handle, CancellationToken ct = default)
    {
        return handle.WaitOneAsync(Timeout.Infinite, ct);
    }
}
