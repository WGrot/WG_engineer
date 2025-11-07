using RestaurantApp.Shared.Models;

namespace RestaurantApp.Domain.Models;

public class MenuItemTag
{
    public int Id { get; set; }
    
    public string Name { get; set; } = string.Empty;
    
    public string ColorHex { get; set; } = "#FFFFFF";
    
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    public virtual ICollection<MenuItem> MenuItems { get; set; } = new HashSet<MenuItem>();
}