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

    public static string GetHashWithIncludeFields(this JsonNode node, List<string>? includedFields = default)
    {
        node = FilterOutFields(node, includedFields, true);
        return node.GetHash();
    }

    public static string GetHashWithExcludeFields(this JsonNode node, List<string>? excludedFields = default)
    {
        node = FilterOutFields(node, excludedFields, false);
        return node.GetHash();
    }

    static JsonNode FilterOutFields(JsonNode node, List<string>? fields = default, bool included = false)
    {
        fields ??= [];
        if (fields.Count == 0)
        {
            return node;
        }

        if (node is JsonObject jsonObject)
        {
            return new JsonObject(jsonObject
                        .Where(kvp => included ? fields.Contains(kvp.Key) : !fields.Contains(kvp.Key))
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
        }

        return node;
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