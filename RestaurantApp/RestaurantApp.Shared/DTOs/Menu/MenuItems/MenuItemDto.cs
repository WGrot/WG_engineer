using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Shared.DTOs.Menu.MenuItems;

public class MenuItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    public int? CategoryId { get; set; }
    public  PriceDto Price { get; set; } = new PriceDto();
    public string CurrencyCode { get; set; } = "PLN";
    public string? ImageUrl { get; set; }
    public string? ThumbnailUrl { get; set; }
    
    public List<MenuItemTagDto> Tags { get; set; } = new List<MenuItemTagDto>();
}