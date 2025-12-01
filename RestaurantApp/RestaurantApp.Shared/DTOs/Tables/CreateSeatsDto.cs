namespace RestaurantApp.Shared.DTOs.Tables;

public class CreateSeatsDto
{
    public int TableId { get; set; }
    public int Count { get; set; }
    public string? SeatNumberPrefix { get; set; } 
    public string? Type { get; set; } = "Standard";
}