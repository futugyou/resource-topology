namespace AwsAgent.ResourceAdapter;

public class AMResourceAdapter : IAMResourceAdapter
{
    private readonly AmazonIdentityManagementServiceClient _iamClient;

    public AMResourceAdapter(AmazonIdentityManagementServiceClient iamClient)
    {
        _iamClient = iamClient;
    }

    public async Task<List<Resource>> ConvertIAMToResource(CancellationToken cancellation)
    {
        var response = new List<Resource>();
        var iamResponse = await _iamClient.ListUsersAsync(cancellation);
        if (iamResponse == null)
        {
            return response;
        }

        foreach (var user in iamResponse.Users)
        {
            response.Add(new Resource()
            {
                Id = user.UserId,
                Label = user.UserName,
                AccountID = user.UserId,
                Arn = user.Arn,
                AvailabilityZone = "",
                AwsRegion = "",
                Configuration = JsonSerializer.Serialize(user),
                ConfigurationItemCaptureTime = DateTime.MinValue,
                ConfigurationItemStatus = "",
                ConfigurationStateID = "",
                ResourceCreationTime = user.CreateDate,
                ResourceID = user.Arn,
                ResourceName = user.UserName,
                ResourceType = "AWS::IAM",
                Tags = [], //TODO: user.Tags,
                Version = "",
                VpcID = "",
                SubnetID = "",
                SubnetIds = [],
                Title = user.UserName,
                SecurityGroups = [],
                LoginURL = "",
                LoggedInURL = "",

            });
        }
        return response;
    }
}