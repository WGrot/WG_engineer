namespace RestaurantApp.Domain.Models;

public class StructuredAddress
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string? PostalCode { get; set; }
    public string Country { get; set; } = "Poland";
    
    public string ToCombinedString()
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(Street))
            parts.Add(Street.Trim());

        if (!string.IsNullOrWhiteSpace(City))
        {
            var cityPart = PostalCode != null 
                ? $"{PostalCode} {City.Trim()}" 
                : City.Trim();
            parts.Add(cityPart);
        }
        else if (!string.IsNullOrWhiteSpace(PostalCode))
        {
            parts.Add(PostalCode);
        }

        if (!string.IsNullOrWhiteSpace(Country))
            parts.Add(Country.Trim());

        return string.Join(", ", parts);
    }
}
