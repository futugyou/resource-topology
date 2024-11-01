namespace AwsAgent.Domain;

public interface IEntity
{
    string Id { get; set; }
    abstract static string GetCollectionName();
}