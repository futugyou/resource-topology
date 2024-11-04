namespace AwsAgent.ResourceAdapter;

public class ConcurrencyLimiter(int maxConcurrentTasks) : IDisposable
{
    private readonly SemaphoreSlim _semaphore = new(maxConcurrentTasks);

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> taskFunc)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await taskFunc();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<T> ExecuteAsync<T, P>(Func<P, Task<T>> taskFunc, P parameter)
    {
        await _semaphore.WaitAsync();
        try
        {
            return await taskFunc(parameter);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _semaphore.Dispose();
    }
}
