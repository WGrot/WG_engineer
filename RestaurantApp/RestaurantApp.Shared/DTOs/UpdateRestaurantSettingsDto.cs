namespace RestaurantApp.Shared.DTOs;

public class UpdateRestaurantSettingsDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public bool ReservationsNeedConfirmation { get; set; } = false;
}