namespace RestaurantApp.Api.Models.DTOs;

public class UpdatePriceDto
{
    public decimal Price { get; set; }
    public string? CurrencyCode { get; set; }
}