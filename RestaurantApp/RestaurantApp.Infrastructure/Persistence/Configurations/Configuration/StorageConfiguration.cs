namespace RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;

public class StorageConfiguration
{
    public required string Endpoint { get; set; }
    
    public string? PublicEndpoint { get; set; } 
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public bool UseSSL { get; set; }
    public required BucketNamesConfiguration BucketNames { get; set; }
}