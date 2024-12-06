
using System.Threading.Tasks.Dataflow;

namespace KubeAgent.Processor;

public class DataflowProcessor : IResourceProcessor
{
    private readonly BufferBlock<Resource> bufferBlock;
    private readonly ActionBlock<List<Resource>> actionBlock;
    private readonly ILogger<DataflowProcessor> logger;

    public DataflowProcessor(ILogger<DataflowProcessor> logger)
    {
        bufferBlock = new BufferBlock<Resource>(new DataflowBlockOptions
        {
            BoundedCapacity = DataflowBlockOptions.Unbounded,
        });
        actionBlock = new ActionBlock<List<Resource>>(async batch =>
        {
            await ProcessBatch(batch, CancellationToken.None);
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 1,
        });
        this.logger = logger;
    }

    public async Task CollectingData(Resource data, CancellationToken cancellation)
    {
        await bufferBlock.SendAsync(data, cancellation);
    }

    public async Task Complete(CancellationToken cancellation)
    {
        bufferBlock.Complete();
        actionBlock.Complete();
        await bufferBlock.Completion;
        await actionBlock.Completion;
    }

    public async Task ProcessingData(CancellationToken cancellation)
    {
        var batch = new List<Resource>();

        while (bufferBlock.TryReceive(out var resource))
        {
            batch.Add(resource);
        }

        if (batch.Count > 0)
        {
            await actionBlock.SendAsync(batch, cancellation);
        }
    }

    private Task ProcessBatch(List<Resource> batch, CancellationToken cancellation)
    {
        //TODO
        logger.LogInformation("Processing batch with {count} items.", batch.Count);
        foreach (var res in batch)
        {
            Console.WriteLine("DataflowProcessor" + "   " + res.Kind + "   " + res.Name + "   " + res.Operate);
        }
        return Task.CompletedTask;
    }
}