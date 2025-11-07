namespace RestaurantApp.Shared.DTOs.Menu;

public class MenuDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}