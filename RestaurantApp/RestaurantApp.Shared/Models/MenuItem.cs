namespace RestaurantApp.Shared.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MenuItemPrice Price { get; set; } = new MenuItemPrice();
    
    public string? ImageUrl { get; set; }
    
    public string? ThumbnailUrl { get; set; }
    public int? MenuId { get; set; }
    public Menu? Menu { get; set; }
    
    // Relacja z kategorią (opcjonalna - item może być bez kategorii)
    public int? CategoryId { get; set; }
    public MenuCategory? Category { get; set; }
}