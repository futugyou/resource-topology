namespace AwsAgent.Extensions;

public static class JsonNodeExtensions
{
    public static string GetHash(this JsonNode node)
    {
        string jsonString = JsonSerializer.Serialize(node, Util.DefaultJsonOptions);
        byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
        byte[] hash = SHA256.HashData(bytes);

        StringBuilder builder = new();
        foreach (byte b in hash)
        {
            builder.Append(b.ToString("x2"));
        }

        return builder.ToString();
    }

    static JsonNode SortJsonNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            var sortedJsonObject = new JsonObject();
            foreach (var property in jsonObject.OrderBy(p => p.Key))
            {
                sortedJsonObject[property.Key] = SortJsonNode(property.Value!);
            }
            return sortedJsonObject;
        }
        else if (node is JsonArray jsonArray)
        {
            var sortedArray = new JsonArray();
            foreach (var item in jsonArray.OrderBy(e => e!.ToString()))
            {
                sortedArray.Add(SortJsonNode(item!));
            }
            return sortedArray;
        }
        else
        {
            return node.DeepClone();
        }
    }
}

public class SortedJsonNodeConverter : JsonConverter<JsonNode>
{
    public override JsonNode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return JsonNode.Parse(ref reader)!;
    }

    public override void Write(Utf8JsonWriter writer, JsonNode value, JsonSerializerOptions options)
    {
        var sortedNode = SortJsonNode(value);
        sortedNode.WriteTo(writer, options);
    }

    private static JsonNode SortJsonNode(JsonNode node)
    {
        if (node is JsonObject jsonObject)
        {
            var sortedJsonObject = new JsonObject();
            foreach (var property in jsonObject.OrderBy(p => p.Key))
            {
                sortedJsonObject[property.Key] = SortJsonNode(property.Value!);
            }
            return sortedJsonObject;
        }
        else if (node is JsonArray jsonArray)
        {
            var sortedArray = new JsonArray();
            foreach (var item in jsonArray.OrderBy(e => e!.ToString()))
            {
                sortedArray.Add(SortJsonNode(item!));
            }
            return sortedArray;
        }
        else
        {
            return node.DeepClone();
        }
    }
}