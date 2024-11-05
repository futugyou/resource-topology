namespace AwsAgent.Extensions;

public static class JsonNodeExtensions
{
    public static string GetHash(this JsonNode node)
    {
        var sortedJsonNode = SortJsonNode(node);
        string jsonString = JsonSerializer.Serialize(sortedJsonNode);
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