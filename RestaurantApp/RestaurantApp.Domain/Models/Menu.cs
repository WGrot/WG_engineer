namespace RestaurantApp.Domain.Models;

public class Menu
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public List<MenuCategory> Categories { get; set; } = new();
    
    public List<MenuItem>? Items { get; set; } = new();
}