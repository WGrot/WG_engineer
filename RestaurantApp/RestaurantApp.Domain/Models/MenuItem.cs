namespace RestaurantApp.Domain.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public MenuItemPrice Price { get; set; } = new MenuItemPrice();
    
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? MenuId { get; set; }
    public Menu? Menu { get; set; }
    
    public int? CategoryId { get; set; }
    public MenuCategory? Category { get; set; }
    
    public virtual ICollection<MenuItemTag> Tags { get; set; } = new HashSet<MenuItemTag>();
    public virtual ICollection<MenuItemVariant> Variants { get; set; } = new HashSet<MenuItemVariant>();

}