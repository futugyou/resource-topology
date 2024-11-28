namespace AwsAgent.Extensions;

public class Util
{
    public static readonly JsonSerializerOptions DefaultJsonOptions = new()
    {
        Converters = { new SortedJsonNodeConverter() },
        WriteIndented = true
    };

    public static JsonNode ConvertToJsonNode(object obj)
    {
        string jsonString = JsonSerializer.Serialize(obj);
        return JsonNode.Parse(jsonString)!;
    }

    public static ResourceTag[] ConvertTag(dynamic tags)
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

    public static string ConvertArnToAccountId(string arn)
    {
        Regex regex = new Regex(Const.AWS_ARN_Pattern);
        Match match = regex.Match(arn);

        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return "";
    }

    public static List<Entity> GetExceptDatas<Entity>(List<Entity> first, List<Entity> second) where Entity : IEntity
    {
        return first.ExceptBy(second.Select(d => d.Id), d => d.Id).ToList();
    }

    public static List<Resource> GetIntersectDatas(List<Resource> first, List<Resource> second)
    {
        return (from a in first
                join b in second on a.Id equals b.Id
                where a.ResourceHash != b.ResourceHash
                select b).ToList();
    }
}