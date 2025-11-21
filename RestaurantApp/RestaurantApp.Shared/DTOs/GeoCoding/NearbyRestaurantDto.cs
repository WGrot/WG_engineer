namespace RestaurantApp.Shared.DTOs.GeoCoding;

public class NearbyRestaurantDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
    public double Distance { get; set; } // Distance in kilometers
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
}