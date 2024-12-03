namespace ResourceContracts;

public record ResourceProcessorEvent
{
    [JsonPropertyName("event_id")]
    public required string EventID { get; set; }
    [JsonPropertyName("provider")]
    public required string Provider { get; set; }

    [JsonPropertyName("insert_resources")]
    public List<Resource> InsertResources { get; set; } = [];

    [JsonPropertyName("delete_resources")]
    public List<string> DeleteResources { get; set; } = [];

    [JsonPropertyName("update_resources")]
    public List<Resource> UpdateResources { get; set; } = [];

    [JsonPropertyName("insert_ships")]
    public List<ResourceRelationship> InsertShips { get; set; } = [];

    [JsonPropertyName("delete_ships")]
    public List<string> DeleteShips { get; set; } = [];
}

public record Resource
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("resourceId")]
    public string ResourceID { get; set; } = "";

    [JsonPropertyName("resourceName")]
    public string ResourceName { get; set; } = "";

    [JsonPropertyName("resourceType")]
    public string ResourceType { get; set; } = "";

    [JsonPropertyName("accountId")]
    public string AccountID { get; set; } = "";

    [JsonPropertyName("region")]
    public string Region { get; set; } = "";

    [JsonPropertyName("availabilityZone")]
    public string AvailabilityZone { get; set; } = "";

    [JsonPropertyName("configuration")]
    public string Configuration { get; set; } = "";

    [JsonPropertyName("resourceCreationTime")]
    public DateTime ResourceCreationTime { get; set; }

    [JsonPropertyName("resourceHash")]
    public string ResourceHash { get; set; } = "";

    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; } = [];

    [JsonPropertyName("resource_url")]
    public string ResourceUrl { get; set; } = "";
}

public record ResourceRelationship
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    [JsonPropertyName("relation")]
    public string Relation { get; set; } = "";

    [JsonPropertyName("sourceId")]
    public string SourceId { get; set; } = "";

    [JsonPropertyName("targetId")]
    public string TargetId { get; set; } = "";
}