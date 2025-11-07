namespace RestaurantApp.Shared.DTOs.Settings;

public class UpdateRestaurantSettingsDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public bool ReservationsNeedConfirmation { get; set; } = false;
}