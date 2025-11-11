namespace RestaurantApp.Shared.DTOs.Menu;

public class UpdateMenuDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

}