namespace RestaurantApp.Shared.DTOs;

public class CreateSeatDto
{
    public string SeatNumber { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string? Type { get; set; } = "Standard";
    public int TableId { get; set; }
}