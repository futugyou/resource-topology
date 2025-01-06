
namespace KubeAgent.Monitor;

public class MonitorOptions
{
    public int CheckIntervalSeconds { get; set; } = 45;
    public int InactiveThresholdMinutes { get; set; } = 9;
}