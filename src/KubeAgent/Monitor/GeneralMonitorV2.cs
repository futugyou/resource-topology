
namespace KubeAgent.Monitor;

public class GeneralMonitorV2(ILogger<GeneralMonitor> logger, IKubernetes client, ProcessorFactory factory, IOptions<MonitorSetting> options, [FromKeyedServices("CRD")] IResourceProcessor flow) : BaseMonitor(logger, factory.GetResourceProcessor()), IResourceMonitor
{
    public async Task MonitorResource(CancellationToken cancellation)
    {
        var monitorSetting = options.Value;
        var resources = monitorSetting.GetMonitorableResources();
        var tasks = new List<Task>();
        foreach (var resource in resources)
        {
            using var childCts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
            var childToken = childCts.Token;
            try
            {
                tasks.Add(InnerMonitorResource(resource.Value.KubeGroup, resource.Value.KubeApiVersion, resource.Value.KubePluralName, resource.Value.ReflectionType, childToken));
            }
            catch (Exception ex)
            {
                logger.LogError("{name} processor error: {error}", resource.Value.KubePluralName, (ex.InnerException ?? ex).Message);
            }
        }

        await Task.WhenAll(tasks);
    }

    Task InnerMonitorResource(string group, string version, string plural, Type targetType, CancellationToken cancellation)
    {
        // TODO: Use cancellation token for restart
        var resources = client.CustomObjects.ListClusterCustomObjectWithHttpMessagesAsync(group, version, plural, watch: true, cancellationToken: cancellation);
        resources.Watch(
            onEvent: (Action<WatchEventType, object>)(async (type, item) =>
            {
                await OnEvent(targetType, type, item, cancellation);
            }),
            onError: async (ex) =>
            {
                if (ex is KubernetesException kubernetesError)
                {
                    if (string.Equals(kubernetesError.Status.Reason, "Expired", StringComparison.Ordinal))
                    {
                        throw ex;
                    }
                }
                await base.HandlerError(InnerMonitorResource, ex, group, version, plural, targetType, cancellation);
            },
            onClosed: () =>
            {
                Console.WriteLine($"Closed: {group} {version} {plural} {targetType}");
            });

        return Task.CompletedTask;
    }

    private async Task OnEvent(Type targetType, WatchEventType type, object item, CancellationToken cancellation)
    {
        var json = JsonSerializer.Serialize(item);
        object? deserializedObject = JsonSerializer.Deserialize(json, targetType);
        if (deserializedObject is IKubernetesObject<V1ObjectMeta> kubernetesObject)
        {
            if (kubernetesObject.Kind == "CustomResourceDefinition" && kubernetesObject is V1CustomResourceDefinition crd)
            {
                foreach (var crdVersion in crd.Spec.Versions)
                {
                    var crdType = crd.Spec.Names.Kind;
                    var crdGroup = crd.Spec.Group;
                    var crdPlural = crd.Spec.Names.Plural;
                    await flow.CollectingData(new Resource
                    {
                        ApiVersion = crdVersion.Name,
                        Kind = crdType,
                        Name = "",
                        UID = "",
                        Configuration = "",
                        Operate = "",
                        Group = crdGroup,
                        Plural = crdPlural,
                    }, cancellation);
                }
            }

            await HandlerResourceChange(type, kubernetesObject, cancellation);
        }
    }
}
