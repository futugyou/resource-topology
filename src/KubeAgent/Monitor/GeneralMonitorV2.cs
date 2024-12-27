
namespace KubeAgent.Monitor;

public class GeneralMonitorV2(ILogger<GeneralMonitor> logger, IKubernetes client, ProcessorFactory factory, IOptions<AgentOptions> options) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var op = options.Value;
        var monitorSetting = new MonitorSetting(op.AllowedResources, op.DeniedResources);

        var resources = monitorSetting.GetMonitorableResources();
        var tasks = new List<Task>();
        foreach (var resource in resources)
        {
            tasks.Add(MonitorResource(resource.Value.KubeGroup, resource.Value.KubeApiVersion, resource.Value.KubePluralName, resource.Value.ReflectionType, cancellation));
        }
        await Task.WhenAll(tasks);
    }

    public async Task MonitorResource(string group, string version, string plural, Type targetType, CancellationToken cancellation)
    {
        var resources = await client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(group, version, plural, watch: true, cancellationToken: cancellation);
        resources.Watch<object, object>(
           onEvent: async (type, item) =>
           {
               var json = JsonSerializer.Serialize(item);
               object? deserializedObject = JsonSerializer.Deserialize(json, targetType);
               if (deserializedObject is IKubernetesObject<V1ObjectMeta> kubernetesObject)
               {
                   await HandlerResourceChange(type, kubernetesObject, cancellation);
               }
           },
           onError: (ex) =>
           {
               // TODO: restart the watch
               logger.LogError("{plural} error: {ex}", plural, (ex.InnerException ?? ex).Message);
           });
    }
}
