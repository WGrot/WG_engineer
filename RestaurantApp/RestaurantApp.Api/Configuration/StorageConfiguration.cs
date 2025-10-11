namespace RestaurantApp.Api.Configuration;

public class StorageConfiguration
{
    public string Endpoint { get; set; }
    public string AccessKey { get; set; }
    public string SecretKey { get; set; }
    public bool UseSSL { get; set; }
    public BucketNamesConfiguration BucketNames { get; set; }
}