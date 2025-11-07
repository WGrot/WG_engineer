namespace RestaurantApp.Shared.DTOs.Tables;

public class CreateTableDto
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Location { get; set; }
    public int RestaurantId { get; set; }
    public int SeatCount { get; set; } = 0;
}