var builder = WebApplication.CreateBuilder(args);

// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&tabs=net-cli#customizing-run-time-behavior-during-build-time-document-generation
if (Assembly.GetEntryAssembly()?.GetName().Name != "GetDocument.Insider")
{
    #region NServiceBus
    var endpointConfiguration = new EndpointConfiguration("resource-manager");
    endpointConfiguration.EnableOpenTelemetry();
    #endregion

    builder.AddServiceDefaults();

    #region NServiceBus
    var connectionString = builder.Configuration.GetConnectionString("rabbitmq");
    var transport = new RabbitMQTransport(RoutingTopology.Conventional(QueueType.Quorum), connectionString);
    var routing = endpointConfiguration.UseTransport(transport);

    endpointConfiguration.UseSerialization<SystemJsonSerializer>();
    endpointConfiguration.EnableInstallers();
    builder.UseNServiceBus(endpointConfiguration);
    #endregion
}


builder.Services.AddOpenApi();

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

// Configure the HTTP request pipeline.

app.MapOpenApi();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "v1");
});

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    Console.WriteLine("forecast count : " + forecast.Length);
    return forecast;
})
.WithName("GetWeatherForecast");

// TODO: some error here https://github.com/dapr/dotnet-sdk/issues/1332#issuecomment-2380321656
app.MapPost("/resource-event-outbox",
[Topic("resource-agent", "resources", "event.type ==\"outbox-resource\"", 1)]
(object resource) =>
{
    var data = JsonSerializer.Serialize(resource);
    Console.WriteLine("outbox-resource received : " + data);
    return Results.Ok(data);
});

app.MapPost("/resource-event",
[Topic("resource-agent", "resources", "event.type ==\"resource\"", 2)]
([FromBody] ResourceProcessorEvent resource) =>
{
    var data = JsonSerializer.Serialize(resource);
    Console.WriteLine("resource received : " + data);
    return TypedResults.Ok(data);
}).WithName("resource-event");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public record ResourceEventWrapper
{
    [property: JsonPropertyName("data")]
    public string Data { get; init; }

    public ResourceEventWrapper(string data)
    {
        Data = data;
    }
}