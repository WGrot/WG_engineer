namespace RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;

public class BucketNamesConfiguration
{
    public required string Images { get; set; }
    public required string Documents { get; set; }
    public required string TempFiles { get; set; }
}