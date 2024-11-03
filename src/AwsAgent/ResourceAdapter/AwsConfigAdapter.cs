
namespace AwsAgent.ResourceAdapter;

public class AwsConfigAdapter(IAmazonConfigService configService) : IResourceAdapter
{
    public async Task<(List<Resource>, List<ResourceRelationship>)> GetResourcAndRelationFromAWS(CancellationToken cancellation)
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
                            and resourceType <> 'AWS::Config::ConfigurationRecorder'
                            and resourceType <> 'AWS::Backup::BackupSelection'
                            and resourceType <> 'AWS::Route53Resolver::ResolverRuleAssociation'
                            and resourceType <> 'AWS::EC2::SubnetRouteTableAssociation'
                    """,
                NextToken = nextToken,
                Limit = 100,
            };
            var response = await configService.SelectResourceConfigAsync(request, cancellation);
            nextToken = response.NextToken;
            if (response?.Results?.Count > 0)
            {
                result.AddRange(response.Results);
            }

        } while (nextToken != null);

        return ConvertConfigStringArrayToResource(result);
    }

    private static (List<Resource>, List<ResourceRelationship>) ConvertConfigStringArrayToResource(List<string> result)
    {
        if (result.Count == 0)
        {
            return ([], []);
        }

        var rawdatastring = "[" + string.Join(",", result) + "]";
        var datas = JsonSerializer.Deserialize<AwsConfigRawData[]>(rawdatastring);
        return ConvertConfigDatasToResource(datas);
    }

    private static (List<Resource>, List<ResourceRelationship>) ConvertConfigDatasToResource(AwsConfigRawData[]? datas)
    {
        if (datas == null || datas.Length == 0)
        {
            return ([], []);
        }

        var result = new List<Resource>(datas.Length);
        var ships = new List<ResourceRelationship>();
        foreach (var data in datas)
        {
            result.Add(new()
            {
                Id = GenerateAwsResourceId(data),
                AccountID = data.AwsAccountID,
                Arn = GenerateAwsResourceId(data),
                AvailabilityZone = data.AvailabilityZone,
                AwsRegion = data.AwsRegion,
                ResourceCreationTime = data.ResourceCreationTime,
                ResourceID = data.ResourceID,
                ResourceName = data.ResourceName,
                ResourceType = data.ResourceType,
                Tags = Util.ConvertTag(data.Tags),
                VpcID = "",
                SubnetID = "",
                SubnetIds = [],
                SecurityGroups = [],
                ResourceUrl = "",
                Configuration = JsonSerializer.Serialize(data.Configuration),
                ConfigurationItemCaptureTime = data.ConfigurationItemCaptureTime,
                ConfigurationItemStatus = data.ConfigurationItemStatus,
                ConfigurationStateID = data.ConfigurationStateID,
                Version = data.ConfigurationItemVersion,
            });
            ships.AddRange(data.Relationships.Select(p => new ResourceRelationship()
            {
                Id = $"{data.ResourceID}-{p.ResourceID}",
                Label = p.Name,
                TargetLabel = p.ResourceName,
                TargetId = p.ResourceID,
                SourceLabel = data.ResourceName,
                SourceId = data.ResourceID,
            }));
        }
        return (result, ships);
    }

    private static string GenerateAwsResourceId(AwsConfigRawData data)
    {
        if (data.ARN?.Length > 0)
        {
            return data.ARN;
        }
        return $"{data.ResourceType.Replace("::", "_").ToLower()}_{data.ResourceID}";
    }
}