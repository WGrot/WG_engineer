namespace RestaurantApp.Shared.Models;

public class Menu
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public List<MenuItem>? Items { get; set; } = new();
}