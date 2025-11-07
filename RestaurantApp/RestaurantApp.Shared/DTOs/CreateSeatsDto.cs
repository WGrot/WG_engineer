namespace RestaurantApp.Shared.DTOs;

public class CreateSeatsDto
{
    public int TableId { get; set; }
    public int Count { get; set; }
    public string? SeatNumberPrefix { get; set; } // Optional custom prefix for seat numbers
    public string? Type { get; set; } = "Standard";
}