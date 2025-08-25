namespace RestaurantApp.Api.Models.DTOs;

public class RestaurantDto
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public List<OpeningHoursDto>? OpeningHours { get; set; }
}