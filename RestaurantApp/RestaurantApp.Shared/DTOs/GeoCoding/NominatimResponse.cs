using System.Text.Json.Serialization;

namespace RestaurantApp.Shared.DTOs.GeoCoding;

public class NominatimResponse
{
    [JsonPropertyName("lat")]
    public string Lat { get; set; }
    
    [JsonPropertyName("lon")]
    public string Lon { get; set; }
    
    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; }
}