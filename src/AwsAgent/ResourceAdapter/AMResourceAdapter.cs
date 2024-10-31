namespace AwsAgent.ResourceAdapter;

public class AMResourceAdapter(IAmazonIdentityManagementService iamClient, IAmazonConfigService configService) : IAMResourceAdapter
{
    public async Task<List<Resource>> ConvertIAMToResource(CancellationToken cancellation)
    {
        var response = new List<Resource>();
        var iamResponse = await iamClient.ListUsersAsync(cancellation);
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

    private static ResourceTag[] ConvertTag(dynamic tags)
    {
        List<ResourceTag> result = [];
        try
        {
            foreach (var tag in tags)
            {
                result.Add(new ResourceTag()
                {
                    Key = tag.Key,
                    Value = tag.Value,
                });
            }
        }
        catch (Exception)
        {
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

    public async Task<List<Resource>> ConvertConfigToResource(CancellationToken cancellation)
    {
        List<string> result = [];
        string? nextToken = null;
        do
        {
            var request = new Amazon.ConfigService.Model.SelectResourceConfigRequest()
            {
                Expression = """
                        SELECT
                            version,
                            accountId,
                            configurationItemCaptureTime,
                            configurationItemStatus,
                            configurationStateId,
                            arn,
                            resourceType,
                            resourceId,
                            resourceName,
                            awsRegion,
                            availabilityZone,
                            tags,
                            relatedEvents,
                            relationships,
                            configuration,
                            supplementaryConfiguration,
                            resourceTransitionStatus,
                            resourceCreationTime
                        WHERE
                            resourceType <> 'AWS::Backup::RecoveryPoint'
                            and resourceType <> 'AWS::CodeDeploy::DeploymentConfig'
                            and resourceType <> 'AWS::RDS::DBSnapshot'
                    """,
                NextToken = nextToken,
            };
            var response = await configService.SelectResourceConfigAsync(request, cancellation);
            nextToken = response.NextToken;
            if (response.Results != null && response.Results.Count > 0)
            {
                result.AddRange(response.Results);
            }

        } while (nextToken == null);

        return convertConfigStringArrayToResource(result);
    }

    private static List<Resource> convertConfigStringArrayToResource(List<string> result)
    {
        if (result.Count == 0)
        {
            return [];
        }

        var rawdatastring = "[" + string.Join(",", result) + "]";
        var datas = JsonSerializer.Deserialize<AwsConfigRawData[]>(rawdatastring);
        return convertConfigDatasToResource(datas);
    }

    private static List<Resource> convertConfigDatasToResource(AwsConfigRawData[]? datas)
    {
        if (datas == null || datas.Length == 0)
        {
            return [];
        }

        var result = new List<Resource>(datas.Length);
        foreach (var data in datas)
        {
            result.Add(new()
            {
                Id = data.ARN,
                Label = data.ResourceName,
                AccountID = data.AwsAccountID,
                Arn = data.ARN,
                AvailabilityZone = data.AvailabilityZone,
                AwsRegion = data.AwsRegion,
                ResourceCreationTime = data.ResourceCreationTime,
                ResourceID = data.ResourceID,
                ResourceName = data.ResourceName,
                ResourceType = data.ResourceType,
                Tags = ConvertTag(data.Tags),
                VpcID = "",
                SubnetID = "",
                SubnetIds = [],
                Title = data.ResourceName,
                SecurityGroups = [],
                ResourceUrl = "",
                Configuration = JsonSerializer.Serialize(data.Configuration),
                ConfigurationItemCaptureTime = data.ConfigurationItemCaptureTime,
                ConfigurationItemStatus = data.ConfigurationItemStatus,
                ConfigurationStateID = data.ConfigurationStateID,
                Version = data.ConfigurationItemVersion,
            });
        }
        return result;
    }
}