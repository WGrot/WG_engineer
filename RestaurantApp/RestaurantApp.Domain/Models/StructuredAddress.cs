namespace RestaurantApp.Domain.Models;

public class StructuredAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Poland";
}