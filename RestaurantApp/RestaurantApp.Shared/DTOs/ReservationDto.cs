using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs;

public class ReservationDto
{
    public DateTime ReservationDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int NumberOfGuests { get; set; }

    // Dane kontaktowe klienta
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    
    public string? Notes { get; set; }

    // Powiązanie z użytkownikiem
    public string UserId { get; set; }
    
    public bool requiresConfirmation { get; set; } = false;
    //Powiązanie z restauracją
    public int RestaurantId { get; set; }
    
    public string RestaurantName { get; set; }
    
    public string RestaurantAddress { get; set; }
}