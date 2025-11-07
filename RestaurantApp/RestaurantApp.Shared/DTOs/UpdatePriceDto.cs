namespace RestaurantApp.Shared.DTOs;

public class UpdatePriceDto
{
    public decimal Price { get; set; }
    public string? CurrencyCode { get; set; }
}