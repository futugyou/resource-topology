using System.Threading.Tasks.Dataflow;

namespace KubeAgent.Processor;

public class GeneralResourceProcessor : IDataProcessor<Resource>, IDisposable, IAsyncDisposable
{
    private readonly BufferBlock<Resource> bufferBlock;
    private readonly ActionBlock<List<Resource>> actionBlock;
    private readonly ILogger<GeneralResourceProcessor> logger;
    private readonly IMapper mapper;
    private readonly IPublisher eventPublisher;
    private bool _isDisposed = false;

    public GeneralResourceProcessor(ILogger<GeneralResourceProcessor> logger, IMapper mapper, [FromKeyedServices("NServiceBus")] IPublisher eventPublisher)
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
        this.mapper = mapper;
        this.eventPublisher = eventPublisher;
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

    private async Task ProcessBatch(List<Resource> batch, CancellationToken cancellation)
    {
        logger.ProcessBatch(batch.Count);
        var events = mapper.Map<ResourceContracts.ResourceProcessorEvent>(batch);
        await eventPublisher.PublishAsync(events, cancellation);
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
            logger.ProcessDisposeError(ex);
        }

        GC.SuppressFinalize(this);
    }
}