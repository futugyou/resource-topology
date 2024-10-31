namespace AwsAgent.ResourceAdapter;

public interface IAMResourceAdapter
{
    Task<List<Resource>> ConvertIAMToResource(CancellationToken cancellation);
    Task<List<Resource>> ConvertConfigToResource(CancellationToken cancellation);
}
