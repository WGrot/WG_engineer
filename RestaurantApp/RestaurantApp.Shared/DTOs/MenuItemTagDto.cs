namespace RestaurantApp.Shared.DTOs;

public class MenuItemTagDto
{
    
    public string Name { get; set; } = string.Empty;
    
    public string ColorHex { get; set; } = "#FFFFFF";
    
    public int RestaurantId { get; set; }

}