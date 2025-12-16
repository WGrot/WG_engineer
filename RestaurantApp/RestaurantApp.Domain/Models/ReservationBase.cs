using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Domain.Models;

public class ReservationBase
{
    public int Id { get; set; }
    public DateTime ReservationDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int NumberOfGuests { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    
    public bool NeedsConfirmation { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
    
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; } = null!;
    
    public string? UserId { get; set; }
    
    public ApplicationUser? User { get; set; } = null;
}