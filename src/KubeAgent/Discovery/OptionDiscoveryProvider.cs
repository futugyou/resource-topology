using System.Reflection;

namespace KubeAgent.Discovery;

// OptionDiscoveryProvider can only support InCluster, so we will set 'default' as cluster name.
public class OptionDiscoveryProvider(IOptionsMonitor<ResourcesSetting> options) : IDiscoveryProvider
{
    public int Priority => 1;

    static Dictionary<string, MonitoredResource> ResourceList()
    {
        var assembly = typeof(V1Pod).Assembly;

        var types = assembly.GetTypes()
            .Where(t => t.IsClass &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IKubernetesObject<>) &&
                            i.GetGenericArguments().FirstOrDefault() == typeof(V1ObjectMeta)));

        var result = new Dictionary<string, MonitoredResource>();

        foreach (var type in types)
        {
            var fieldNames = new[] { "KubeApiVersion", "KubeKind", "KubeGroup", "KubePluralName" };

            var fieldValues = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && fieldNames.Contains(f.Name))
                .ToDictionary(f => f.Name, f => f.GetValue(null)?.ToString());

            if (!fieldValues.ContainsKey("KubeKind")) continue;

            var resourceInfo = new MonitoredResource
            {
                ClusterName = "default",
                KubeApiVersion = fieldValues.GetValueOrDefault("KubeApiVersion") ?? "",
                KubeKind = fieldValues.GetValueOrDefault("KubeKind") ?? "",
                KubeGroup = fieldValues.GetValueOrDefault("KubeGroup") ?? "",
                KubePluralName = fieldValues.GetValueOrDefault("KubePluralName") ?? "",
                Source = "Configurations",
                ReflectionType = type,
            };

            result[resourceInfo.KubeKind] = resourceInfo;
        }

        return result;
    }

    public Task<IEnumerable<MonitoredResource>> GetMonitoredResourcesAsync(CancellationToken cancellation)
    {
        var setting = options.CurrentValue;
        Dictionary<string, MonitoredResource> list = ResourceList();

        if (setting.AllowedResources.Count == 0)
        {
            setting.AllowedResources = [.. setting.DefaultAllowedResources];
        }

        var allowedSet = setting.AllowedResources.Count > 0 ? setting.AllowedResources : [.. list.Keys];

        var monitorableResources = allowedSet
            .Where(resource => !setting.DeniedResources.Contains(resource))
            .ToHashSet();

        var currentWatchList = list
            .Where(p => monitorableResources.Contains(p.Key))
            .ToDictionary(p => p.Key, p => p.Value);

        foreach (var detail in setting.Details)
        {
            if (detail.Namespaced && detail.Namespaces.Length > 0 && currentWatchList.TryGetValue(detail.Resource, out var resource))
            {
                currentWatchList.Remove(detail.Resource);

                foreach (var ns in detail.Namespaces)
                {
                    var namespacedResource = new MonitoredResource
                    {
                        ClusterName = "default",
                        KubeApiVersion = resource.KubeApiVersion,
                        KubeKind = resource.KubeKind,
                        KubeGroup = resource.KubeGroup,
                        KubePluralName = resource.KubePluralName,
                        Source = resource.Source,
                        ReflectionType = resource.ReflectionType,
                        Namespace = ns
                    };
                    currentWatchList[$"{resource.KubeKind}-{ns}"] = namespacedResource;
                }
            }
        }

        return Task.FromResult<IEnumerable<MonitoredResource>>(currentWatchList.Values);
    }
}
