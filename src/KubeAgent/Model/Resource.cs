namespace KubeAgent.Model;
public class Resource
{
    //TODO
    public string Cluster { get; set; } = "";
    public required string ApiVersion { get; set; }
    public required string Kind { get; set; }
    public required string Name { get; set; }
    public string Group { get; set; } = "";
    public string Plural { get; set; } = "";
    public required string UID { get; set; }
    public DateTime ResourceCreationTime { get; set; }
    public required string Configuration { get; set; }// json
    public Dictionary<string, string> Tags { get; set; } = [];
    public required string Operate { get; set; }
}