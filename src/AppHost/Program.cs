var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.AwsAgent>("aws");

builder.AddProject<Projects.KubeAgent>("k8s");

builder.AddProject<Projects.ResourceManager>("manager");

builder.Build().Run();
