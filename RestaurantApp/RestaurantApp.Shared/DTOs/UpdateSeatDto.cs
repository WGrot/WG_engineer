namespace RestaurantApp.Shared.DTOs;

public class UpdateSeatDto
{
    public string SeatNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? Type { get; set; }
}