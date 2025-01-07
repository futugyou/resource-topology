
namespace KubeAgent.Services;

public class JsonSerializerService : ISerializer
{
    private static readonly JsonSerializerOptions DefaultJsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, DefaultJsonSerializerOptions);
    }

    public T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultJsonSerializerOptions);
    }

    public object? Deserialize(string json, Type targetType)
    {
        return JsonSerializer.Deserialize(json, targetType, DefaultJsonSerializerOptions);
    }
}
