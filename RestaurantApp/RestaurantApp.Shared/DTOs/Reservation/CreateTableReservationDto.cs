namespace RestaurantApp.Shared.DTOs.Reservation;

public class CreateTableReservationDto
{
    public int TableId { get; set; }
    public int RestaurantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public int NumberOfGuests { get; set; }
    
    // Dane kontaktowe
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    
    // Szczegóły rezerwacji
    public DateTime ReservationDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    
    public string? Notes { get; set; }
    public bool RequiresConfirmation { get; set; } = false;
}