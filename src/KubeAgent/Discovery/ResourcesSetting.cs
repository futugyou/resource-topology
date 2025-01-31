
namespace KubeAgent.Discovery;

public class ResourcesSetting()
{
    public List<string> AllowedResources { get; set; } = [];
    public List<string> DeniedResources { get; set; } = [];
    public ResourceConfig[] Details { get; set; } = [];
    public List<string> DefaultAllowedResources { get; set; } =
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
        "PodDisruptionBudget",
        "PriorityClass",
        "Node",
    ];
}

public class ResourceConfig
{
    public string Resource { get; set; } = "";
    public bool Namespaced { get; set; }
    public string[] Namespaces { get; set; } = [];
}