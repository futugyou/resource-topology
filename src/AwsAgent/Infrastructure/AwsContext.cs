namespace AwsAgent.Infrastructure;

public class AwsContext : DbContext
{
    public DbSet<Resource> Resources { get; init; }
    public DbSet<ResourceRelationship> ResourceRelationships { get; init; }
    
    public AwsContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Resource>().ToCollection(Resource.GetCollectionName());
        modelBuilder.Entity<ResourceRelationship>().ToCollection(ResourceRelationship.GetCollectionName());
    }
}