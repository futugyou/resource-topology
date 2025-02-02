using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

DaprSidecarOptions awsSidecarOptions = new()
{
    AppId = "aws-agent",
    DaprGrpcPort = 50001,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};
builder.AddProject<Projects.AwsAgent>("aws-agent").WithDaprSidecar(awsSidecarOptions);

builder.AddProject<Projects.KubeAgent>("k8s");

DaprSidecarOptions managerSidecarOptions = new()
{
    AppId = "resource-manager",
    DaprGrpcPort = 50001,
    AppPort = 5000,
    MetricsPort = 9090,
    LogLevel = "warn",
    EnableApiLogging = true,
};
builder.AddProject<Projects.ResourceManager>("resource-manager").WithDaprSidecar(managerSidecarOptions);

builder.Build().Run();
