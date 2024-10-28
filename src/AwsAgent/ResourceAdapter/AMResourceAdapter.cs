namespace AwsAgent.ResourceAdapter;

public class AMResourceAdapter : IAMResourceAdapter
{
    static readonly string pattern = @"arn:aws:[^:]*:(\d{12}):";
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
                Id = user.Arn,
                Label = user.UserName,
                AccountID = ConvertArnToAccountId(user.Arn),
                Arn = user.Arn,
                AvailabilityZone = "Not Applicable",
                AwsRegion = "Not Applicable",
                ResourceCreationTime = user.CreateDate,
                ResourceID = user.Arn,
                ResourceName = user.UserName,
                ResourceType = "AWS::IAM::User",
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
        Regex regex = new Regex(pattern);
        Match match = regex.Match(arn);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
    }
}