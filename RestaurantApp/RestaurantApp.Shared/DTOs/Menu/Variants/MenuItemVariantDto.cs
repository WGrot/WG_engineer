namespace RestaurantApp.Shared.DTOs.Menu.Variants;

public class MenuItemVariantDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public int MenuItemId { get; set; }
}