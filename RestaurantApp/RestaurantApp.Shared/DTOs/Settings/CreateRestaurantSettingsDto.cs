namespace RestaurantApp.Shared.DTOs.Settings;

public class CreateRestaurantSettingsDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; } // FK do Restaurant
    public bool ReservationsNeedConfirmation { get; set; } = false;
}