namespace RestaurantApp.Shared.DTOs;

public class OpenStatusDto
{
    public bool IsOpen { get; set; }
    public string? Message { get; set; }
    public string DayOfWeek { get; set; } = string.Empty;
    public string CheckedTime { get; set; } = string.Empty;
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public bool? IsClosed { get; set; }
}