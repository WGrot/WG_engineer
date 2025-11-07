namespace RestaurantApp.Shared.DTOs.Tables;

public class UpdateTableDto
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Location { get; set; }
}