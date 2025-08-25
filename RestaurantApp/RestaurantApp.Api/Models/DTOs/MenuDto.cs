namespace RestaurantApp.Api.Models.DTOs;

public class MenuDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}