namespace RestaurantApp.Shared.DTOs.Settings;

public class CreateRestaurantSettingsDto
{
    public int RestaurantId { get; set; } 
    public bool ReservationsNeedConfirmation { get; set; } = false;
}