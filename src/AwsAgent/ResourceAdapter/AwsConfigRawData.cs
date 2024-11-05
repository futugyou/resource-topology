namespace AwsAgent.ResourceAdapter;

public class AwsConfigRawData
{
    [JsonPropertyName("relatedEvents")]
    public string[] RelatedEvents { get; set; } = [];
    [JsonPropertyName("relationships")]
    public Relationship[] Relationships { get; set; } = [];
    [JsonPropertyName("configuration")]
    public required JsonNode Configuration { get; set; }
    [JsonPropertyName("tags")]
    public ConfigTag[] Tags { get; set; } = [];
    [JsonPropertyName("version")]
    public string ConfigurationItemVersion { get; set; } = "";
    [JsonPropertyName("configurationItemCaptureTime")]
    public DateTime ConfigurationItemCaptureTime { get; set; } = DateTime.MinValue;
    [JsonPropertyName("configurationStateId")]
    public string ConfigurationStateID { get; set; } = "";
    [JsonPropertyName("accountId")]
    public string AwsAccountID { get; set; } = "";
    [JsonPropertyName("configurationItemStatus")]
    public string ConfigurationItemStatus { get; set; } = "";
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "";
    [JsonPropertyName("resourceId")]
    public string ResourceID { get; set; } = "";
    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = "";
    [JsonPropertyName("arn")]
    public string ARN { get; set; } = "";
    [JsonPropertyName("awsRegion")]
    public string AwsRegion { get; set; } = "";
    [JsonPropertyName("availabilityZone")]
    public string AvailabilityZone { get; set; } = "";
    [JsonPropertyName("resourceCreationTime")]
    public DateTime ResourceCreationTime { get; set; } = DateTime.MinValue;
}

public class Relationship
{
    [JsonPropertyName("resourceId")]
    public string ResourceID { get; set; } = "";
    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = "";
    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "";
    [JsonPropertyName("relationshipName")]
    public string Name { get; set; } = "";
}

public class ConfigTag
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = "";
    [JsonPropertyName("value")]
    public string Value { get; set; } = "";
}
