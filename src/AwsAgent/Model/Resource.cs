
namespace AwsAgent.Model;

public record IncrementResource(List<Resource> DBResources, List<ResourceRelationship> DBShips, List<Resource> AWSResources, List<ResourceRelationship> AWSShips);
public record DifferentialResourcesRecord(List<Resource> InsertDatas, List<Resource> DeleteDatas, List<Resource> UpdateDatas, List<ResourceRelationship> InsertShipDatas, List<ResourceRelationship> DeleteShipDatas)
{
    public bool HasChange() => InsertDatas.Count != 0 || DeleteDatas.Count != 0 || UpdateDatas.Count != 0 || InsertShipDatas.Count != 0 || DeleteShipDatas.Count != 0;
}
public record  ResourceAndShip(List<Resource> Resources, List<ResourceRelationship> Ships);