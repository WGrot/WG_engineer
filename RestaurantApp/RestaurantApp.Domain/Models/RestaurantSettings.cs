namespace RestaurantApp.Domain.Models;

public class RestaurantSettings
{
    public int Id { get; set; }
    public int RestaurantId { get; set; } 
    public bool ReservationsNeedConfirmation { get; set; } = false;
    
    public TimeSpan MinReservationDuration { get; set; } = TimeSpan.FromMinutes(30);
    public TimeSpan MaxReservationDuration { get; set; } = TimeSpan.FromHours(3);
    
    public TimeSpan MinAdvanceBookingTime { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan MaxAdvanceBookingTime { get; set; } = TimeSpan.FromDays(30);
    
    public int MaxGuestsPerReservation { get; set; } = 10;
    public int MinGuestsPerReservation { get; set; } = 1;
    public int ReservationsPerUserLimit { get; set; } = 5;
    
    public Restaurant Restaurant { get; set; }
}