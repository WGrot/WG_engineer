namespace RestaurantApp.Shared.DTOs.Menu.MenuItems;

public class PriceDto
{
    public decimal Amount { get; set; } = 0.0m;
    public string CurrencyCode { get; set; } = "PLN";
}