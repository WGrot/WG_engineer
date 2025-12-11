namespace RestaurantApp.Shared.DTOs.Settings;

public class UpdateRestaurantSettingsDto
{
    public int RestaurantId { get; set; }
    public bool ReservationsNeedConfirmation { get; set; }
    
    public TimeSpan MinReservationDuration { get; set; }
    public TimeSpan MaxReservationDuration { get; set; }
    
    public TimeSpan MinAdvanceBookingTime { get; set; }
    public TimeSpan MaxAdvanceBookingTime { get; set; }
    
    public int MaxGuestsPerReservation { get; set; }
    public int MinGuestsPerReservation { get; set; }
    public int ReservationsPerUserLimit { get; set; }
}