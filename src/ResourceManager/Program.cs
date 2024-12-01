using ResourceContracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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
.WithName("GetWeatherForecast")
.WithOpenApi();

// app.MapPost("/resources", [Topic("resource-agent", "resources")] ([FromBody] ResourceProcessorEvent resource) =>
// TODO: some error here https://github.com/dapr/dotnet-sdk/issues/1332#issuecomment-2380321656
app.MapPost("/resources", [Topic("resource-agent", "resources")] (object resource) =>
{
    var data = JsonSerializer.Serialize(resource);
    Console.WriteLine("Subscriber received : " + data);
    return Results.Ok(data);
});

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