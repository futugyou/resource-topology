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
                ConfigurationItemCaptureTime = DateTime.MinValue,
                ConfigurationItemStatus = "",
                ConfigurationStateID = "",
                Version = "",
            });
        }

        //TODO: how to get relship
        return (response, []);
    }
}