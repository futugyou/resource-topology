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

        var resources = new List<Resource>(datas.Length);
        var ships = new List<ResourceRelationship>();
        foreach (var data in datas)
        {
            resources.Add(new()
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
                ResourceHash = GenerateResourceHash(data),
            });
            ships.AddRange(data.Relationships.Select(p => new ResourceRelationship()
            {
                Id = "",
                Label = p.Name,
                TargetLabel = p.ResourceType,
                TargetId = p.ResourceID,
                SourceLabel = data.ResourceType,
                SourceId = data.ResourceID,
            }));
        }
        ConversionShipData(resources, ships);
        return (resources, ships);
    }

    private static string GenerateResourceHash(AwsConfigRawData data)
    {
        // // TODO: use data.Configuration or data.ConfigurationItemCaptureTime
        // string jsonString = JsonSerializer.Serialize(data.Configuration);
        // byte[] bytes = Encoding.UTF8.GetBytes(jsonString);
        // byte[] hash = SHA256.HashData(bytes);
        // StringBuilder builder = new();
        // foreach (byte b in hash)
        // {
        //     builder.Append(b.ToString("x2"));
        // }
        // return builder.ToString();

        // return data.ConfigurationItemCaptureTime.ToString();

        return data.Configuration.GetHash();
    }

    private static void ConversionShipData(List<Resource> resources, List<ResourceRelationship> ships)
    {
        foreach (var ship in ships)
        {
            var source = resources.FirstOrDefault(p => p.ResourceID == ship.SourceId && p.ResourceType == ship.SourceLabel);
            var target = resources.FirstOrDefault(p => p.ResourceID == ship.TargetId && p.ResourceType == ship.TargetLabel);

            if (source != null && target != null)
            {
                ship.Id = $"{source.Id}##{target.Id}";
                ship.TargetId = target.Id;
                ship.SourceId = source.Id;
            }
        }

        ships.RemoveAll(p => string.IsNullOrEmpty(p.Id));
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