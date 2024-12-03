namespace AwsAgent.Option;

public class ServiceOption
{
    public required string AccessKeyId { get; set; }
    public required string SecretAccessKey { get; set; }
    public required string Region { get; set; }
    public required string DBName { get; set; }
    // second
    public required int WorkerInterval { get; set; } = 60;
    public required int WorkerTime { get; set; } = 20;
    // single exec like github action/cmd/k8s cron job 
    // continuous exec like k8s deployment/k8s job ...
    public required bool RunSingle { get; set; } = false;
    public required bool AwsconfigSupported { get; set; } = true;
    public required bool DaprWorkflowSupported { get; set; } = false;
    public required bool DaprStateOutboxSupported { get; set; } = false;
    public required int MaxConcurrentAdapters { get; set; } = 5;
}