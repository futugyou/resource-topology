var builder = Host.CreateApplicationBuilder(args);

builder.AddApplicationServices();

var host = builder.Build();

await host.RunAsync();