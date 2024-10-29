namespace AwsAgent.Domain;

[BsonIgnoreExtraElements]
public class ResourceRelationship
{
    [BsonElement("_id")]
    public required string Id { get; set; }

    [BsonElement("label")]
    public required string Label { get; set; }

    [BsonElement("sourceId")]
    public required string SourceId { get; set; }

    [BsonElement("sourceLabel")]
    public required string SourceLabel { get; set; }

    [BsonElement("targetId")]
    public required string TargetId { get; set; }

    [BsonElement("targetLabel")]
    public required string TargetLabel { get; set; }
}
