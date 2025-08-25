namespace RestaurantApp.Api.Models.DTOs;

public class UpdateTableDto
{
    public string TableNumber { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string? Location { get; set; }
}