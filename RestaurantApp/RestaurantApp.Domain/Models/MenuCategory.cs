namespace RestaurantApp.Domain.Models;

public class MenuCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; 
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } 
    public bool IsActive { get; set; } = true;
    

    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;

    public List<MenuItem> Items { get; set; } = new();
}