using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Reservation;

public class ReservationDto
{
    public int Id { get; set; }
    public DateTime ReservationDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int NumberOfGuests { get; set; }
    
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public ReservationStatusEnumDto StatusEnumDto { get; set; } = ReservationStatusEnumDto.Pending;
    
    public string? Notes { get; set; }
    
    public string UserId { get; set; }
    
    public bool requiresConfirmation { get; set; } = false;
    public int RestaurantId { get; set; }
    
    public string RestaurantName { get; set; }
    
    public string RestaurantAddress { get; set; }
}