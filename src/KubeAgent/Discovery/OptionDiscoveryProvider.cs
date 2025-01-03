using System.Reflection;

namespace KubeAgent.Discovery;

public class OptionDiscoveryProvider(IOptionsMonitor<MonitorSetting> options) : IDiscoveryProvider
{
    Dictionary<string, MonitoredResource> monitoredResourceList = [];
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
                KubeApiVersion = fieldValues.GetValueOrDefault("KubeApiVersion") ?? "",
                KubeKind = fieldValues.GetValueOrDefault("KubeKind") ?? "",
                KubeGroup = fieldValues.GetValueOrDefault("KubeGroup") ?? "",
                KubePluralName = fieldValues.GetValueOrDefault("KubePluralName") ?? "",
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

        if (monitoredResourceList.Count == 0)
        {
            monitoredResourceList = new Dictionary<string, MonitoredResource>(currentWatchList);
            return Task.FromResult<IEnumerable<MonitoredResource>>(currentWatchList.Values);
        }

        var deletedResources = monitoredResourceList
            .Where(kv => !currentWatchList.ContainsKey(kv.Key))
            .Select(kv =>
            {
                kv.Value.Operate = "delete"; 
                return kv.Value;
            });

        var combinedWatchList = currentWatchList.Values.Concat(deletedResources).ToList();

        monitoredResourceList = new Dictionary<string, MonitoredResource>(currentWatchList);

        return Task.FromResult<IEnumerable<MonitoredResource>>(combinedWatchList);
    }
}
