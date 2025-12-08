using RestaurantApp.Shared.DTOs.GeoCoding;

namespace RestaurantApp.Shared.DTOs.Restaurant;

public class RestaurantBasicInfoDto
{
    public string Name { get; set; }
    public string Address { get; set; }
    public string Description { get; set; }
}