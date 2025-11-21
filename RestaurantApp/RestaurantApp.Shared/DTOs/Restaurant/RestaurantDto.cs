using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.OpeningHours;
using RestaurantApp.Shared.DTOs.Settings;

namespace RestaurantApp.Shared.DTOs.Restaurant;

public class RestaurantDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? Description { get; set; }
    
    // Godziny otwarcia
    public List<OpeningHoursDto>? OpeningHours { get; set; }
    
    // Zdjęcia
    public string? ProfileUrl { get; set; }
    public string? ProfileThumbnailUrl { get; set; }
    public List<string>? PhotosUrls { get; set; }
    public List<string>? PhotosThumbnailsUrls { get; set; }
    
    public StructuresAddressDto ? StructuredAddress { get; set; }
    
    public GeoLocationDto? Location { get; set; }
    
    public MenuDto? Menu { get; set; }
    
    // Oceny i recenzje
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalRatings1Star { get; set; }
    public int TotalRatings2Star { get; set; }
    public int TotalRatings3Star { get; set; }
    public int TotalRatings4Star { get; set; }
    public int TotalRatings5Star { get; set; }
    
    public SettingsDto Settings { get; set; }
}