namespace KubeAgent.Monitor;

public interface IResourceMonitor
{
    Task MonitorResource(CancellationToken cancellation);
}

public abstract class BaseMonitor(ILogger<BaseMonitor> logger, IResourceProcessor processor)
{
    readonly ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            MaxRetryAttempts = int.MaxValue,
            DelayGenerator = args =>
            {
                var delay = 60d;
                if (args.AttemptNumber < 6)
                {
                    delay = Math.Pow(2, args.AttemptNumber);
                }

                var msg = args.Outcome.Exception?.Message ?? "";
                logger.LogWarning("MonitorResource Retry, AttemptNumber: {AttemptNumber}, Delay: {delay}s ,ex: {msg}", args.AttemptNumber, delay, msg);
                return new ValueTask<TimeSpan?>(TimeSpan.FromSeconds(delay));
            }
        })
        .Build();

    protected virtual async Task HandlerError(Func<CancellationToken, Task> fn, string methodName, Exception ex, CancellationToken cancellation)
    {
        logger.LogError("{name} error: {ex}", methodName, (ex.InnerException ?? ex).Message);

        switch (ex)
        {
            case KubernetesException e when e.Status.Code is (int)HttpStatusCode.Gone:
                // TODO: add k8s resourceVersion
                break;
        }

        await pipeline.ExecuteAsync(async (cancel) => await fn(cancel), cancellation);
    }

    protected virtual async Task HandlerError(Func<string, string, string, Type, CancellationToken, Task> fn, Exception ex, string group, string version, string plural, Type targetType, CancellationToken cancellation)
    {
        logger.LogError("{plural} error: {ex}", plural, (ex.InnerException ?? ex).Message);
        switch (ex)
        {
            case KubernetesException e when e.Status.Code is (int)HttpStatusCode.Gone:
                // TODO: add k8s resourceVersion
                break;
        }

        await pipeline.ExecuteAsync(async (cancel) => await fn(group, version, plural, targetType, cancel), cancellation);
    }

    protected virtual async Task HandlerResourceChange(WatchEventType type, IKubernetesObject<V1ObjectMeta> item, CancellationToken cancellation)
    {
        if (type == WatchEventType.Bookmark)
        {
            return;
        }

        var jsonItem = item as dynamic;
        var res = new Resource
        {
            ApiVersion = item.ApiVersion,
            Kind = item.Kind,
            Name = item.Name(),
            UID = item.Uid(),
            Configuration = JsonSerializer.Serialize(jsonItem),
            Operate = type.ToString(),
        };
        await processor.CollectingData(res, cancellation);
    }
}
