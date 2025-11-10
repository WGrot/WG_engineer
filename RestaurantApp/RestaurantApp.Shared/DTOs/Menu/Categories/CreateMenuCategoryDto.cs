namespace RestaurantApp.Shared.DTOs.Menu.Categories;

public class CreateMenuCategoryDto
{
    public int MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    
}