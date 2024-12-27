using System.Reflection;

namespace KubeAgent;

public class MonitorSetting(List<string> AllowedResources, List<string> DeniedResources)
{
    private readonly string[] defaultAllowedResources =
    [
        "Namespace",
        "Pod",
        "Service",
        "Deployment",
        "ReplicaSet",
        "StatefulSet",
        "DaemonSet",
        "Job",
        "CronJob",
        "ConfigMap",
        "Secret",
        "ServiceAccount",
        "Role",
        "RoleBinding",
        "ClusterRole",
        "ClusterRoleBinding",
        "NetworkPolicy",
        "Ingress",
        "PersistentVolume",
        "PersistentVolumeClaim",
        "StorageClass",
        "VolumeAttachment", 
        "LimitRange",
        "ResourceQuota",
        "PodSecurityPolicy",
        "PodDisruptionBudget",
        "PriorityClass",
        "Node",
        // "Event",
    ];

    public Dictionary<string, KubeResourceInfo> GetMonitorableResources()
    {
        var list = ResourceList();

        if (AllowedResources.Count == 0)
        {
            AllowedResources = [.. defaultAllowedResources];
        }

        var allowedSet = AllowedResources.Count > 0 ? AllowedResources : [.. list.Keys];

        var monitorableResources = allowedSet
            .Where(resource => !DeniedResources.Contains(resource))
            .ToHashSet();

        return list.Where(p => monitorableResources.Contains(p.Key))
                   .ToDictionary(p => p.Key, p => p.Value);
    }


    static Dictionary<string, KubeResourceInfo> ResourceList()
    {
        var assembly = typeof(V1Pod).Assembly;

        var types = assembly.GetTypes()
            .Where(t => t.IsClass &&
                        t.GetInterfaces().Any(i =>
                            i.IsGenericType &&
                            i.GetGenericTypeDefinition() == typeof(IKubernetesObject<>) &&
                            i.GetGenericArguments().FirstOrDefault() == typeof(V1ObjectMeta)));

        var result = new Dictionary<string, KubeResourceInfo>();

        foreach (var type in types)
        {
            var fieldNames = new[] { "KubeApiVersion", "KubeKind", "KubeGroup", "KubePluralName" };

            var fieldValues = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => f.IsLiteral && !f.IsInitOnly && fieldNames.Contains(f.Name))
                .ToDictionary(f => f.Name, f => f.GetValue(null)?.ToString());

            if (!fieldValues.ContainsKey("KubeKind")) continue;

            var resourceInfo = new KubeResourceInfo
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
}

public class KubeResourceInfo
{
    public string KubeApiVersion { get; set; } = "";
    public string KubeKind { get; set; } = "";
    public string KubeGroup { get; set; } = "";
    public string KubePluralName { get; set; } = "";
    public required Type ReflectionType { get; set; }
}
