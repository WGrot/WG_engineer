namespace RestaurantApp.Api.Models.DTOs;

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

    public string? Notes { get; set; }

    // Powiązanie z użytkownikiem
    public string UserId { get; set; }
    
    //Powiązanie z restauracją
    public int RestaurantId { get; set; }
}