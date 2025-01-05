
namespace KubeAgent.Services;

public class RestartResourceTracker : AbstractChannelProcessor<RestartContext>, IRestartResourceTracker
{
    Dictionary<string, RestartContext> restartResourceList = [];

    public async Task AddRestartResource(RestartContext context, CancellationToken cancellation)
    {
        await CollectingData(context, cancellation);
    }

    public async Task<IEnumerable<RestartContext>> GetRestartResources(CancellationToken cancellation)
    {
        restartResourceList = [];
        await ProcessingData(cancellation);
        return restartResourceList.Values;
    }

    override protected Task ProcessBatch(List<RestartContext> batch, CancellationToken cancellation)
    {
        restartResourceList = batch.ToDictionary(x => x.ResourceId);
        return Task.CompletedTask;
    }
}
