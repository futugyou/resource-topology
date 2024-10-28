namespace ResourceAdapter;

public interface IAMResourceAdapter
{
    Task<List<Resource>> ConvertIAMToResource(CancellationToken cancellation);
}
