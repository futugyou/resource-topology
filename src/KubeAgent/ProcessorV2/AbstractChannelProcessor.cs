using System.Threading.Channels;

namespace KubeAgent.ProcessorV2;

public abstract class AbstractChannelProcessor<T> : IDisposable, IAsyncDisposable
{
    readonly Channel<T> channel = Channel.CreateUnbounded<T>();
    private bool _isDisposed = false;
    public virtual async Task CollectingData(T data, CancellationToken cancellation)
    {
        if (_isDisposed)
        {
            return;
        }
        await channel.Writer.WriteAsync(data, cancellation);
    }

    public virtual async Task Complete(CancellationToken cancellation)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        channel.Writer.Complete();
        await channel.Reader.Completion;
    }

    public virtual async Task ProcessingData(CancellationToken cancellation)
    {
        if (_isDisposed)
        {
            return;
        }

        var batch = new List<T>();

        while (channel.Reader.TryRead(out var data))
        {
            batch.Add(data);
        }

        if (batch.Count > 0)
        {
            await ProcessBatch(batch, cancellation);
        }
    }

    protected abstract Task ProcessBatch(List<T> batch, CancellationToken cancellation);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (disposing)
        {
            channel.Writer.Complete();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        channel.Writer.Complete();

        try
        {
            await channel.Reader.Completion;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during async disposal: {ex.Message}");
        }

        GC.SuppressFinalize(this);
    }
}
