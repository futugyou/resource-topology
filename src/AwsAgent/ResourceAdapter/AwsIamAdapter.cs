namespace AwsAgent.ResourceAdapter;

public class AwsIamAdapter(IAmazonIdentityManagementService iamClient) : IResourceAdapter
{
    public async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
        var response = new List<Resource>();
        var iamResponse = await iamClient.ListUsersAsync(cancellation);
        if (iamResponse == null)
        {
            return ([], []);
        }

        foreach (var user in iamResponse.Users)
        {
            response.Add(new Resource()
            {
                Id = user.Arn,
                AccountID = Util.ConvertArnToAccountId(user.Arn),
                Arn = user.Arn,
                AvailabilityZone = Const.AWS_Global_Zone,
                AwsRegion = Const.AWS_Global_Region,
                ResourceCreationTime = user.CreateDate,
                ResourceID = user.UserId,
                ResourceName = user.UserName,
                ResourceType = Const.AWS_IAM_User,
                Tags = Util.ConvertTag(user.Tags),
                VpcID = "",
                SubnetID = "",
                SubnetIds = [],
                SecurityGroups = [],
                ResourceUrl = "",
                Configuration = JsonSerializer.Serialize(user),
                ConfigurationItemCaptureTime = user.CreateDate,
                ConfigurationItemStatus = "",
                ConfigurationStateID = "",
                Version = "",
                // TODO: When using the XXXClient, there is currently no suitable method to make the hash consistent with the config service,
                // So it is randomly generated for all operations.
                ResourceHash = Guid.NewGuid().ToString(),
            });
        }

        //TODO: how to get relship
        return (response, []);
    }
}

public record AwsIamConfig
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("groupList")]
    public List<string> GroupList { get; set; } = [];

    [JsonPropertyName("userPolicyList")]
    public List<UserPolicy> UserPolicyList { get; set; } = [];

    [JsonPropertyName("attachedManagedPolicies")]
    public List<UserPolicy> AttachedManagedPolicies { get; set; } = [];

    [JsonPropertyName("userName")]
    public string UserName { get; set; } = "";

    [JsonPropertyName("arn")]
    public string Arn { get; set; } = "";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = "";

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("tags")]
    public List<Tag> Tags { get; set; } = [];
}

public class UserPolicy
{
    [JsonPropertyName("policyDocument")]
    public string PolicyDocument { get; set; } = "";

    [JsonPropertyName("policyName")]
    public string PolicyName { get; set; } = "";
}