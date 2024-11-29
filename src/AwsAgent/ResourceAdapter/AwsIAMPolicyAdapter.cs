namespace AwsAgent.ResourceAdapter;

public class AwsIAMPolicyAdapter(IAmazonIdentityManagementService iamClient) : IResourceAdapter
{
    public async Task<ResourceAndShip> GetResourcAndRelationFromAWS(CancellationToken cancellation)
    {
        var response = new List<Resource>();
        var iamResponse = await iamClient.ListPoliciesAsync(cancellation);
        if (iamResponse == null)
        {
            return new ResourceAndShip([], []);
        }
        foreach (var policy in iamResponse.Policies)
        {
            var config = new PolicyConfig
            {
                Arn = policy.Arn,
                AttachmentCount = policy.AttachmentCount,
                CreateDate = policy.CreateDate,
                DefaultVersionId = policy.DefaultVersionId,
                IsAttachable = policy.IsAttachable,
                Path = policy.Path,
                PermissionsBoundaryUsageCount = policy.PermissionsBoundaryUsageCount,
                PolicyId = policy.PolicyId,
                PolicyName = policy.PolicyName,
                // TODO： need remove PolicyVersionList from both here and awsconfig 
                // PolicyVersionList = [],
                UpdateDate = policy.UpdateDate,
            };

            var configNode = Util.ConvertToJsonNode(config);
            response.Add(new Resource()
            {
                Id = policy.Arn,
                AccountID = Util.ConvertArnToAccountId(policy.Arn),
                Arn = policy.Arn,
                AvailabilityZone = Const.AWS_Global_Zone,
                AwsRegion = Const.AWS_Global_Region,
                ResourceCreationTime = policy.CreateDate,
                ResourceID = policy.PolicyId,
                ResourceName = policy.PolicyName,
                ResourceType = Const.AWS_IAM_Policy,
                Tags = Util.ConvertTag(policy.Tags),
                VpcID = "",
                SubnetID = "",
                SubnetIds = [],
                SecurityGroups = [],
                ResourceUrl = "",
                Configuration = JsonSerializer.Serialize(config, Util.DefaultJsonOptions),
                ConfigurationItemCaptureTime = policy.CreateDate,
                ConfigurationItemStatus = "",
                ConfigurationStateID = "",
                ResourceHash = configNode.GetHash(),
            });
        }

        return new ResourceAndShip(response, []);
    }
}

public class PolicyVersionList
{
    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("document")]
    public string Document { get; set; } = "";

    [JsonPropertyName("isDefaultVersion")]
    public bool IsDefaultVersion { get; set; }

    [JsonPropertyName("versionId")]
    public string VersionId { get; set; } = "";
}

public class PolicyConfig
{
    [JsonPropertyName("arn")]
    public string Arn { get; set; } = "";

    [JsonPropertyName("attachmentCount")]
    public double AttachmentCount { get; set; }

    [JsonPropertyName("createDate")]
    public DateTime CreateDate { get; set; }

    [JsonPropertyName("defaultVersionId")]
    public string DefaultVersionId { get; set; } = "";

    [JsonPropertyName("isAttachable")]
    public bool IsAttachable { get; set; }

    [JsonPropertyName("path")]
    public string Path { get; set; } = "";

    [JsonPropertyName("permissionsBoundaryUsageCount")]
    public double PermissionsBoundaryUsageCount { get; set; }

    [JsonPropertyName("policyId")]
    public string PolicyId { get; set; } = "";

    [JsonPropertyName("policyName")]
    public string PolicyName { get; set; } = "";

    // [JsonPropertyName("policyVersionList")]
    // public List<PolicyVersionList> PolicyVersionList { get; set; } = [];

    [JsonPropertyName("updateDate")]
    public DateTime UpdateDate { get; set; }
}

