namespace RestaurantApp.Domain.Models;

public class MenuCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty; // np. "Napoje", "Dania główne", "Przystawki"
    public string? Description { get; set; }
    public int DisplayOrder { get; set; } // Kolejność wyświetlania kategorii
    public bool IsActive { get; set; } = true;
    
    // Relacja z Menu
    public int MenuId { get; set; }
    public Menu Menu { get; set; } = null!;
    
    // Lista pozycji w kategorii
    public List<MenuItem> Items { get; set; } = new();
}