using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.GeoCoding;

namespace RestaurantApp.Infrastructure.Services;

public class NominatimGeocodingService : IGeocodingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NominatimGeocodingService> _logger;
    
    public NominatimGeocodingService(HttpClient httpClient, ILogger<NominatimGeocodingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    
    public async Task<(double? lat, double? lon)> GeocodeAddressAsync(string address)
    {
        try
        {
            var encodedAddress = Uri.EscapeDataString(address);
            var url = $"search?q={encodedAddress}&format=json&limit=1&addressdetails=1";
            
            return await ExecuteGeocodeRequestAsync(url, address);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding address: {Address}", address);
            return (null, null);
        }
    }
    
    public async Task<(double? lat, double? lon)> GeocodeStructuredAsync(
        string street, 
        string city, 
        string postalCode = null, 
        string country = "Polska")
    {
        try
        {
            var queryParams = new List<string>
            {
                $"street={Uri.EscapeDataString(street)}",
                $"city={Uri.EscapeDataString(city)}",
                $"country={Uri.EscapeDataString(country)}",
                "format=json",
                "limit=1",
                "addressdetails=1"
            };
            
            if (!string.IsNullOrEmpty(postalCode))
            {
                queryParams.Add($"postalcode={Uri.EscapeDataString(postalCode)}");
            }
            
            var url = $"search?{string.Join("&", queryParams)}";
            var addressForLog = $"{street}, {city}, {country}";
            
            return await ExecuteGeocodeRequestAsync(url, addressForLog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error geocoding structured address");
            return (null, null);
        }
    }
    
    private async Task<(double? lat, double? lon)> ExecuteGeocodeRequestAsync(string url, string addressForLog)
    {
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Nominatim API returned status code: {StatusCode} for address: {Address}", 
                response.StatusCode, addressForLog);
            return (null, null);
        }
        
        var content = await response.Content.ReadAsStringAsync();
        var results = JsonSerializer.Deserialize<List<NominatimResponse>>(content);
        
        if (results?.Any() == true)
        {
            var result = Enumerable.First<NominatimResponse>(results);
            if (double.TryParse(result.Lat, NumberStyles.Any, CultureInfo.InvariantCulture, out var lat) &&
                double.TryParse(result.Lon, NumberStyles.Any, CultureInfo.InvariantCulture, out var lon))
            {
                _logger.LogInformation("Successfully geocoded address: {Address} to ({Lat}, {Lon})", 
                    addressForLog, lat, lon);
                return (lat, lon);
            }
        }
        
        _logger.LogWarning("No results found for address: {Address}", addressForLog);
        return (null, null);
    }
}