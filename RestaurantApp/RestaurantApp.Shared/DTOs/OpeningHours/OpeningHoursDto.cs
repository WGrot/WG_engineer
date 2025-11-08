namespace RestaurantApp.Shared.DTOs.OpeningHours;

public class OpeningHoursDto
{
    public DayOfWeek DayOfWeek { get; set; } // Używamy enum zamiast int
    public TimeOnly OpenTime { get; set; }    // Używamy TimeOnly zamiast string
    public TimeOnly CloseTime { get; set; }   // Używamy TimeOnly zamiast string
    public bool IsClosed { get; set; }
    
    public int RestaurantId { get; set; }
    
    public bool IsOpenAt(TimeOnly time)
    {
        if (IsClosed) return false;
        
        if (CloseTime > OpenTime)
            return time >= OpenTime && time <= CloseTime;
        else 
            return time >= OpenTime || time <= CloseTime;
    }
}