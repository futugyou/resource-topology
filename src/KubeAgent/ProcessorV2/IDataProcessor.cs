using System.Threading.Channels;

namespace KubeAgent.ProcessorV2;

public interface IDataProcessor<T>
{
    Task CollectingData(T data, CancellationToken cancellation);
    Task ProcessingData(CancellationToken cancellation);
    Task Complete(CancellationToken cancellation);
}

public class AbstractChannelProcessor<T> : IDataProcessor<T>
{
    readonly Channel<T> channel = Channel.CreateUnbounded<T>();

    public virtual async Task CollectingData(T data, CancellationToken cancellation)
    {
        await channel.Writer.WriteAsync(data, cancellation);
    }

    public virtual async Task Complete(CancellationToken cancellation)
    {
        channel.Writer.Complete();
        await channel.Reader.Completion;
    }

    public virtual async Task ProcessingData(CancellationToken cancellation)
    {
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

    protected virtual Task ProcessBatch(List<T> batch, CancellationToken cancellation)
    {
        return Task.CompletedTask;
    }
}
