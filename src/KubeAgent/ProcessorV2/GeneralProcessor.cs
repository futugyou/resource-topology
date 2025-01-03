using System.Threading.Tasks.Dataflow;

namespace KubeAgent.ProcessorV2;

public class GeneralProcessor : IDataProcessor<Resource>, IDisposable, IAsyncDisposable
{
    private readonly BufferBlock<Resource> bufferBlock;
    private readonly ActionBlock<List<Resource>> actionBlock;
    private readonly ILogger<GeneralProcessor> logger;
    private bool _isDisposed = false;

    public GeneralProcessor(ILogger<GeneralProcessor> logger)
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
        if (_isDisposed)
        {
            return;
        }
        await bufferBlock.SendAsync(data, cancellation);
    }

    public async Task Complete(CancellationToken cancellation)
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;
        bufferBlock.Complete();
        actionBlock.Complete();
        await bufferBlock.Completion;
        await actionBlock.Completion;
    }

    public async Task ProcessingData(CancellationToken cancellation)
    {
        if (_isDisposed)
        {
            return;
        }
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
        logger.LogInformation("processing batch with {count} items.", batch.Count);
        foreach (var res in batch)
        {
            logger.LogInformation("resource processor handling: {kind} {name} {operate}", res.Kind, res.Name, res.Operate);
        }
        return Task.CompletedTask;
    }

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
            bufferBlock.Complete();
            actionBlock.Complete();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }
        _isDisposed = true;

        bufferBlock.Complete();
        actionBlock.Complete();

        try
        {
            await bufferBlock.Completion;
            await actionBlock.Completion;
        }
        catch (Exception ex)
        {
            logger.LogError("Error during async disposal: {message}", ex.Message);
        }

        GC.SuppressFinalize(this);
    }
}