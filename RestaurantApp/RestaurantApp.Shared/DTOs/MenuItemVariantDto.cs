namespace RestaurantApp.Shared.DTOs;

public class MenuItemVariantDto
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;


    public int MenuItemId { get; set; }
}