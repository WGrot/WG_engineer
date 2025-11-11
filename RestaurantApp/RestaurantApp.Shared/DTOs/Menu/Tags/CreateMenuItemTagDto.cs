namespace RestaurantApp.Shared.DTOs.Menu.Tags;

public class CreateMenuItemTagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    
    public string ColorHex { get; set; } = "#FFFFFF";
    
    public int RestaurantId { get; set; }
}