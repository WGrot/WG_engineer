namespace RestaurantApp.Shared.DTOs.OpeningHours;

public class OpeningHoursDto
{
    public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, etc.
    public string OpenTime { get; set; } = string.Empty; // Format: "HH:mm" e.g., "09:00"
    public string CloseTime { get; set; } = string.Empty; // Format: "HH:mm" e.g., "22:00"
    public bool IsClosed { get; set; }
}