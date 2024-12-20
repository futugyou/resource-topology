
using System.Threading.Channels;

namespace KubeAgent.Processor;

public class ChannelProcessor(ILogger<ChannelProcessor> logger) : IResourceProcessor
{
    readonly Channel<Resource> channel = Channel.CreateUnbounded<Resource>();

    public async Task CollectingData(Resource data, CancellationToken cancellation)
    {
        await channel.Writer.WriteAsync(data, cancellation);
    }

    public async Task Complete(CancellationToken cancellation)
    {
        channel.Writer.Complete();
        await channel.Reader.Completion;
    }

    public async Task ProcessingData(CancellationToken cancellation)
    {
        var batch = new List<Resource>();

        while (channel.Reader.TryRead(out var data))
        {
            batch.Add(data);
        }

        if (batch.Count > 0)
        {
            await ProcessBatch(batch, cancellation);
        }
    }

    private Task ProcessBatch(List<Resource> batch, CancellationToken cancellation)
    {
        //TODO
        logger.LogInformation("Processing batch with {count} items.", batch.Count);
        foreach (var res in batch)
        {
            Console.WriteLine("ChannelProcessor" + "   " + res.Kind + "   " + res.Name + "   " + res.Operate);
        }
        return Task.CompletedTask;
    }
}