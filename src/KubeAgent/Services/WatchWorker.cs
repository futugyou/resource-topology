
using k8s.Models;

namespace KubeAgent.Services;

public class WatchWorker(ILogger<Worker> logger, IServiceProvider servicerovider, IKubernetes client) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Kube agent watch worker running at: {time}", DateTimeOffset.Now);

        try
        {
            var namespaces = await client.CoreV1.ListNamespaceWithHttpMessagesAsync(watch: true, cancellationToken: stoppingToken);
            namespaces.Watch<V1Namespace, V1NamespaceList>((type, item) =>
            {
                Console.WriteLine(type + "   " + item.Name());
            });

            var podlistResp = client.CoreV1.ListPodForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: stoppingToken);
            await foreach (var (type, item) in podlistResp.WatchAsync<V1Pod, V1PodList>())
            {
                 Console.WriteLine(type + "   " + item.Name());
            }
        }
        catch (Exception ex)
        {
            logger.LogError("Kube agent watch worker running at: {time}, and get an error: {error}", DateTimeOffset.Now, (ex.InnerException ?? ex).Message);
        }

        logger.LogInformation("Kube agent watch woorker end at: {time}", DateTimeOffset.Now);
    }
}