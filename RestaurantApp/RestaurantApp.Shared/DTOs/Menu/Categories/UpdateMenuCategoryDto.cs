using RestaurantApp.Shared.DTOs.Menu.MenuItems;

namespace RestaurantApp.Shared.DTOs.Menu.Categories;

public class UpdateMenuCategoryDto
{
    public int Id { get; set; }
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool? IsActive { get; set; } = true;
    
    public List<MenuItemDto>? Items { get; set; } = new();
}