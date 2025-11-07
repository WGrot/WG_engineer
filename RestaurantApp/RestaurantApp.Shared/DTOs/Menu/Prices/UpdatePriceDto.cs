namespace RestaurantApp.Shared.DTOs.Menu.Prices;

public class UpdatePriceDto
{
    public decimal Price { get; set; }
    public string? CurrencyCode { get; set; }
}