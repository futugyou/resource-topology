
using System.Threading.Tasks.Dataflow;

namespace KubeAgent.Processor;

public class DataflowProcessor : IResourceProcessor
{
    private readonly BufferBlock<Resource> bufferBlock;
    private readonly ILogger<ResourceProcessor> logger;

    public DataflowProcessor(ILogger<ResourceProcessor> logger)
    {
        bufferBlock = new BufferBlock<Resource>(new DataflowBlockOptions
        {
            BoundedCapacity = DataflowBlockOptions.Unbounded,
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
        await bufferBlock.Completion;
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
            await ProcessBatch(batch, cancellation);
        }
    }

    private Task ProcessBatch(List<Resource> batch, CancellationToken cancellation)
    {
        //TODO
        logger.LogInformation("Processing batch with {count} items.", batch.Count);
        foreach (var res in batch)
        {
            Console.WriteLine(res.ResourceType + "   " + res.Name);
        }
        return Task.CompletedTask;
    }
}