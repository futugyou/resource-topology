namespace KubeAgent.Monitor;

public class InactiveResourceChecker(ILogger<InactiveResourceChecker> logger,
                                     IRestartResourceTracker restartResourceTracker,
                                     IResourceMonitor monitor,
                                     IOptionsMonitor<MonitorOptions> options) : IInactiveResourceChecker
{
    private int _timerstart = 0;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(options.CurrentValue.CheckIntervalSeconds);
    private readonly TimeSpan _inactiveThreshold = TimeSpan.FromMinutes(options.CurrentValue.InactiveThresholdMinutes);

    public void StartInactiveCheckTask(CancellationToken cancellation)
    {
        if (Interlocked.CompareExchange(ref _timerstart, 1, 0) == 0)
        {
            Task.Run(async () =>
            {
                while (!cancellation.IsCancellationRequested)
                {
                    await Task.Delay(_checkInterval, cancellation);
                    CheckInactiveResources(cancellation);
                }
            }, cancellation);
        }
    }

    private async void CheckInactiveResources(CancellationToken cancellation)
    {
        var now = DateTime.Now;
        var currentList = await monitor.GetWatcherListAsync(cancellation);
        foreach (var watcher in currentList)
        {
            if (now - watcher.LastActiveTime > _inactiveThreshold)
            {
                logger.MonitorTimeout(watcher.ResourceId);
                await RestartResource(watcher.ResourceId, cancellation);
            }
        }
    }

    private async Task RestartResource(string resourceId, CancellationToken cancellation)
    {
        await restartResourceTracker.AddRestartResource(new RestartContext { ResourceId = resourceId }, cancellation);
    }
}
