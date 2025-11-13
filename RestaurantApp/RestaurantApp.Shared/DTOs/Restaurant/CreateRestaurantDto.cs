namespace RestaurantApp.Shared.DTOs.Restaurant;

public class CreateRestaurantDto
{
    public string OwnerId { get; set; }= string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
}