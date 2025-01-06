
namespace KubeAgent.Monitor;

public interface IInactiveResourceChecker
{
    void StartInactiveCheckTask(CancellationToken cancellation);
}
