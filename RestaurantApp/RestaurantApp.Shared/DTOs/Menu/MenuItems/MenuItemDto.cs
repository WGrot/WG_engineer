namespace RestaurantApp.Shared.DTOs.Menu.MenuItems;

public class MenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CurrencyCode { get; set; } = "PLN";
    public string? ImagePath { get; set; }
}