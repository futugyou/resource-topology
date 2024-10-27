namespace Domain;

[BsonIgnoreExtraElements]
public class Resource
{
    [BsonElement("_id")]
    public required string Id { get; set; }

    [BsonElement("label")]
    public required string Label { get; set; }

    [BsonElement("accountId")]
    public required string AccountID { get; set; }

    [BsonElement("arn")]
    public required string Arn { get; set; }

    [BsonElement("availabilityZone")]
    public required string AvailabilityZone { get; set; }

    [BsonElement("awsRegion")]
    public required string AwsRegion { get; set; }

    [BsonElement("configuration")]
    public required string Configuration { get; set; }

    [BsonElement("configurationItemCaptureTime")]
    public required DateTime ConfigurationItemCaptureTime { get; set; }

    [BsonElement("configurationItemStatus")]
    public required string ConfigurationItemStatus { get; set; }

    [BsonElement("configurationStateId")]
    public required string ConfigurationStateID { get; set; }

    [BsonElement("resourceCreationTime")]
    public required DateTime ResourceCreationTime { get; set; }

    [BsonElement("resourceId")]
    public required string ResourceID { get; set; }

    [BsonElement("resourceName")]
    public required string ResourceName { get; set; }

    [BsonElement("resourceType")]
    public required string ResourceType { get; set; }

    [BsonElement("tags")]
    public required string[] Tags { get; set; }

    [BsonElement("version")]
    public required string Version { get; set; }

    [BsonElement("vpcId")]
    public required string VpcID { get; set; }

    [BsonElement("subnetId")]
    public required string SubnetID { get; set; }

    [BsonElement("subnetIds")]
    public required string[] SubnetIds { get; set; }

    [BsonElement("title")]
    public required string Title { get; set; }

    [BsonElement("securityGroups")]
    public required string[] SecurityGroups { get; set; }

    [BsonElement("loginURL")]
    public required string LoginURL { get; set; }

    [BsonElement("loggedInURL")]
    public required string LoggedInURL { get; set; }
}
