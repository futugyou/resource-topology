namespace AwsAgent.ResourceAdapter;

public class AMResourceAdapter : IAMResourceAdapter
{

    private readonly IAmazonIdentityManagementService _iamClient;

    public AMResourceAdapter(IAmazonIdentityManagementService iamClient)
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
                Id = user.Arn,
                Label = user.UserName,
                AccountID = ConvertArnToAccountId(user.Arn),
                Arn = user.Arn,
                AvailabilityZone = Util.AWS_Global_Zone,
                AwsRegion = Util.AWS_Global_Region,
                ResourceCreationTime = user.CreateDate,
                ResourceID = user.UserId,
                ResourceName = user.UserName,
                ResourceType = Util.AWS_IAM_User,
                Tags = ConvertTag(user.Tags),
                VpcID = "",
                SubnetID = "",
                SubnetIds = [],
                Title = user.UserName,
                SecurityGroups = [],
                ResourceUrl = "",
                Configuration = JsonSerializer.Serialize(user),
                ConfigurationItemCaptureTime = DateTime.MinValue,
                ConfigurationItemStatus = "",
                ConfigurationStateID = "",
                Version = "",
            });
        }
        return response;
    }

    private static string[] ConvertTag(List<Amazon.IdentityManagement.Model.Tag> tags)
    {
        List<string> result = [];
        foreach (var tag in tags)
        {
            var t = string.Join("|", [tag.Key, tag.Value]);
            result.Add(t);
        }
        return [.. result];
    }

    private static string ConvertArnToAccountId(string arn)
    {
        Regex regex = new Regex(Util.AWS_ARN_Pattern);
        Match match = regex.Match(arn);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
    }
}