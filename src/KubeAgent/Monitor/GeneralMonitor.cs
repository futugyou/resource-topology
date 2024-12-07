
namespace KubeAgent.Monitor;

public class GeneralMonitor(ILogger<NodeMonitor> logger, IKubernetes client, ProcessorFactory factory) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public Task MonitorResource(CancellationToken cancellation)
    {
        List<dynamic> gl = [
            new G<V1ConfigMap, V1ConfigMapList>(
            ListResourcesFunc: cancellation => client.CoreV1.ListConfigMapForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        ),
        new G<V1DaemonSet, V1DaemonSetList>(
            ListResourcesFunc: cancellation => client.AppsV1.ListDaemonSetForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        ),
        new G<V1Deployment, V1DeploymentList>(
            ListResourcesFunc: cancellation => client.AppsV1.ListDeploymentForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        ),
        new G<V1Namespace, V1NamespaceList>(
            ListResourcesFunc: cancellation => client.CoreV1.ListNamespaceWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        ),
        new G<V1Node, V1NodeList>(
            ListResourcesFunc: cancellation => client.CoreV1.ListNodeWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        ),
        new G<V1Service, V1ServiceList>(
            ListResourcesFunc: cancellation => client.CoreV1.ListServiceForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        ),
        new G<V1StatefulSet, V1StatefulSetList>(
            ListResourcesFunc: cancellation => client.AppsV1.ListStatefulSetForAllNamespacesWithHttpMessagesAsync(watch: true, cancellationToken: cancellation),
            ResourceChangeFunc: HandlerResourceChange,
            ErrorHandlerFunc: HandlerError
        )
        ];
        var list = new List<Task>(gl.Count);
        foreach (var g in gl)
        {
            list.Add(g.MonitorResource(cancellation));
        }

        return Task.WhenAll(list);
    }
}

public class G<T, L>(
    Func<CancellationToken, Task<HttpOperationResponse<L>>> ListResourcesFunc,
    Func<WatchEventType, T, CancellationToken, Task> ResourceChangeFunc,
    Func<Func<CancellationToken, Task>, string, Exception, CancellationToken, Task> ErrorHandlerFunc)
    where T : IKubernetesObject<V1ObjectMeta>, IValidate where L : IKubernetesObject<V1ListMeta>, IItems<T>, IValidate
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var resources = await ListResourcesFunc(cancellation);
        resources.Watch<T, L>(
           onEvent: async (type, item) => await ResourceChangeFunc(type, item, cancellation),
           onError: async (ex) => await ErrorHandlerFunc(MonitorResource, typeof(T).Name, ex, cancellation));
    }
}
