
namespace KubeAgent.Services;

public interface IKubernetesClientProvider
{
    Task<Dictionary<string, IKubernetes>> GetKubernetesClientsAsync(CancellationToken cancellation);
}
