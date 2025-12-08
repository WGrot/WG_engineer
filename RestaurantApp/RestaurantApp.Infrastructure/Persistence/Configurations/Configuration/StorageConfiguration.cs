namespace RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;

public class StorageConfiguration
{
    public string Endpoint { get; set; }
    
    public string? PublicEndpoint { get; set; } 
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public bool UseSSL { get; set; }
    public BucketNamesConfiguration BucketNames { get; set; }
}