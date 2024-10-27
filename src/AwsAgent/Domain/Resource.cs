using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Domain;

[BsonIgnoreExtraElements]
public class Resource
{
    public required string Id { get; set; }

    public required string Label { get; set; }

    [BsonElement("accountId")]
    public required string AccountID { get; set; }

    public required string AvailabilityZone { get; set; }

    public required string AwsRegion { get; set; }
}
