namespace KubeAgent;

public class AgentOptions
{
    public string? KubeconfigPath { get; set; }
    public required int WorkerInterval { get; set; } = 60;
    public required int WorkerTime { get; set; } = 20;
}
