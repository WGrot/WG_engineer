namespace RestaurantApp.Shared.DTOs.Settings;

public class UpdateRestaurantSettingsDto
{
    public int RestaurantId { get; set; }
    public bool ReservationsNeedConfirmation { get; set; } = false;
}