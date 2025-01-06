
namespace KubeAgent.Monitor;

public class MonitorOptions
{
    public int CheckIntervalSeconds { get; set; } = 45;
    public int InactiveThresholdMinutes { get; set; } = 9;
    public bool CustomResourced { get; set; } = true;
}