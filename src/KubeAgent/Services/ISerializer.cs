
namespace KubeAgent.Services;

public interface ISerializer
{
    string Serialize<T>(T obj);
    T? Deserialize<T>(string json);
    object? Deserialize(string json, Type targetType);
}
