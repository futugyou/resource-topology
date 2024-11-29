using Amazon.IdentityManagement.Model;
using Group = Amazon.IdentityManagement.Model.Group;

namespace AwsAgent.ResourceAdapter;

public class AwsIAMUserAdapter(IAmazonIdentityManagementService iamClient) : IResourceAdapter
{
    public async Task<ResourceAndShip> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
        var response = new List<Resource>();
        var iamResponse = await iamClient.ListUsersAsync(cancellation);
        if (iamResponse == null)
        {
            return new ResourceAndShip([], []);
        }

        foreach (var user in iamResponse.Users)
        {
            var groups = await GetUserGroup(user.UserName, cancellation);
            var attachedPolicies = await GetAttachedUserPolicy(user.UserName, cancellation);
            // TODO: fill UserPolicyList 
            var config = new AwsIamConfig
            {
                Path = user.Path,
                GroupList = groups.Select(p => p.GroupName).ToList(),
                UserName = user.UserName,
                UserId = user.UserId,
                Arn = user.Arn,
                CreateDate = user.CreateDate,
                Tags = user.Tags.Select(p => new ConfigTag { Key = p.Key, Value = p.Value }).ToList(),
                AttachedManagedPolicies = attachedPolicies.Select(p => new UserPolicy { PolicyArn = p.PolicyArn, PolicyName = p.PolicyName }).ToList(),
            };

            var configNode = Util.ConvertToJsonNode(config);
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
                Configuration = JsonSerializer.Serialize(config, Util.DefaultJsonOptions),
                ConfigurationItemCaptureTime = user.CreateDate,
                ConfigurationItemStatus = "",
                ConfigurationStateID = "",
                ResourceHash = configNode.GetHash(),
            });
        }

        //TODO: how to get relship
        return new ResourceAndShip(response, []);
    }

    private async Task<List<Group>> GetUserGroup(string userName, CancellationToken cancellation)
    {
        var request = new ListGroupsForUserRequest(userName);
        var response = await iamClient.ListGroupsForUserAsync(request, cancellation);
        if (response == null)
        {
            return [];
        }

        return response.Groups;
    }
    private async Task<List<AttachedPolicyType>> GetAttachedUserPolicy(string userName, CancellationToken cancellation)
    {
        var request = new ListAttachedUserPoliciesRequest()
        {
            UserName = userName,
        };
        var response = await iamClient.ListAttachedUserPoliciesAsync(request, cancellation);
        if (response == null)
        {
            return [];
        }

        return response.AttachedPolicies;
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
    public List<ConfigTag> Tags { get; set; } = [];
}

public class UserPolicy
{
    [JsonPropertyName("policyDocument")]
    public string PolicyDocument { get; set; } = "";

    [JsonPropertyName("policyName")]
    public string PolicyName { get; set; } = "";

    [JsonPropertyName("policyArn")]
    public string PolicyArn { get; set; } = "";
}