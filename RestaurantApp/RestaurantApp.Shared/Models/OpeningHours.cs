using System.ComponentModel.DataAnnotations.Schema;

namespace RestaurantApp.Shared.Models;

[ComplexType]
public class OpeningHours
{
    public int Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpenTime { get; set; }
    public TimeOnly CloseTime { get; set; }
    public bool IsClosed { get; set; } = false; 
    
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public bool IsOpenAt(TimeOnly time)
    {
        if (IsClosed) return false;
        
        if (CloseTime > OpenTime)
            return time >= OpenTime && time <= CloseTime;
        else 
            return time >= OpenTime || time <= CloseTime;
    }
}