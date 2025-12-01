namespace RestaurantApp.Shared.DTOs.OpeningHours;

public class OpeningHoursDto
{
    public DayOfWeek DayOfWeek { get; set; } 
    public TimeOnly OpenTime { get; set; }    
    public TimeOnly CloseTime { get; set; } 
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