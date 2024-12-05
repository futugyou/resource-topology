
namespace KubeAgent.Services;

public class Worker(ILogger<Worker> logger, IServiceProvider servicerovider, IKubernetes client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {

            var scope = servicerovider.CreateAsyncScope();
            var optionsMonitor = scope.ServiceProvider.GetRequiredService<IOptionsMonitor<AgentOptions>>();
            var serviceOption = optionsMonitor.CurrentValue!;
            logger.LogInformation("Kube agent worker running at: {time}", DateTimeOffset.Now);

            try
            {
                var namespaces = client.CoreV1.ListNamespace();
                foreach (var ns in namespaces.Items)
                {
                    Console.WriteLine(ns.Metadata.Name);
                    var list = client.CoreV1.ListNamespacedPod(ns.Metadata.Name);
                    foreach (var item in list.Items)
                    {
                        Console.WriteLine(item.Metadata.Name);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError("Kube agent worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
            }

            logger.LogInformation("Kube agent woorker end at: {time}", DateTimeOffset.Now);
            await Task.Delay(1000 * serviceOption.WorkerInterval, stoppingToken);
        }
    }
}
