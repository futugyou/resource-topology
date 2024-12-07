
namespace KubeAgent.Monitor;

public interface IResourceMonitor
{
    Task MonitorResource(CancellationToken cancellation);
}

public abstract class BaseMonitor(ILogger<BaseMonitor> logger)
{
    protected readonly ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
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
}