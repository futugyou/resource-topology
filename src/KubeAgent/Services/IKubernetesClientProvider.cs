
namespace KubeAgent.Services;

public interface IKubernetesClientProvider
{
    Task<List<IKubernetes>> GetKubernetesClientsAsync(CancellationToken cancellation);
}