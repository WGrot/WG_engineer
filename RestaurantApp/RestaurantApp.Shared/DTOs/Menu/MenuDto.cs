using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.MenuItems;

namespace RestaurantApp.Shared.DTOs.Menu;

public class MenuDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    
    public List<MenuCategoryDto> Categories { get; set; } = new();
    
    public List<MenuItemDto>? Items { get; set; } = new();
}