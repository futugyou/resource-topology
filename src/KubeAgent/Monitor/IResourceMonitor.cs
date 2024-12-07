
using k8s.Autorest;

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
        await pipeline.ExecuteAsync(async (cancel) => await fn(cancel), cancellation);
    }

    protected virtual async Task HandlerResourceChange(WatchEventType type, IKubernetesObject<V1ObjectMeta> item, CancellationToken cancellation)
    {
        var res = new Resource
        {
            ApiVersion = item.ApiVersion,
            Kind = item.Kind,
            Name = item.Name(),
            UID = item.Uid(),
            Configuration = JsonSerializer.Serialize(item),
            Operate = type.ToString(),
        };
        await processor.CollectingData(res, cancellation);
    }
}