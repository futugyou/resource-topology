namespace AwsAgent.Services;
public record ResourceProcessorEvent(
    [property: JsonPropertyName("insert_resources")] List<Resource> InsertResources,
    [property: JsonPropertyName("delete_resources")] List<string> DeleteResources,
    [property: JsonPropertyName("update_resources")] List<Resource> UpdateResources
);