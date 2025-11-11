using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Shared.DTOs.Menu.MenuItems;

public class UpdateMenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PriceDto Price { get; set; }
    public string CurrencyCode { get; set; } = "PLN";
    
    public List<MenuItemTagDto> Tags { get; set; } = new List<MenuItemTagDto>();
}