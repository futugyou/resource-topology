using Aspire.Hosting.Dapr;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AwsAgent>("aws").WithDaprSidecar();

builder.AddProject<Projects.KubeAgent>("k8s");

builder.AddProject<Projects.ResourceManager>("manager").WithDaprSidecar();

builder.Build().Run();
