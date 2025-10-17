namespace RestaurantApp.Shared.Models;

public class MenuItemVariant
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public decimal Price { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string Description { get; set; } = string.Empty;

    public int MenuItemId { get; set; }
    public virtual MenuItem MenuItem { get; set; } = null!;
}