using RestaurantApp.Shared.Models;

namespace RestaurantApp.Domain.Models;

public class Seat
{
    public int Id { get; set; }
    public string SeatNumber { get; set; } = string.Empty; 
    public bool IsAvailable { get; set; } = true;
    public string? Type { get; set; } 
    
    public int TableId { get; set; }
    public Table Table { get; set; } = null!;
}