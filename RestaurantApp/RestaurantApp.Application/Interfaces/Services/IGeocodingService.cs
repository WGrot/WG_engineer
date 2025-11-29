namespace RestaurantApp.Application.Interfaces.Services;

public interface IGeocodingService
{
    Task<(double? lat, double? lon)> GeocodeAddressAsync(string address);
    Task<(double? lat, double? lon)> GeocodeStructuredAsync(
        string street, 
        string city, 
        string postalCode = null, 
        string country = "Poland");
}