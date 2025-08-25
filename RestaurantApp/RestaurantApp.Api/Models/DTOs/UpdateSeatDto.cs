namespace RestaurantApp.Api.Models.DTOs;

public class UpdateSeatDto
{
    public string SeatNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Type { get; set; }
}