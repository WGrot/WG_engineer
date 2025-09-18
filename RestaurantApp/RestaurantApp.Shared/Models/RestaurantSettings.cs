namespace RestaurantApp.Shared.Models;

public class RestaurantSettings
{
    public int Id { get; set; }
    public int RestaurantId { get; set; } // FK do Restaurant
    public bool ReservationsNeedConfirmation { get; set; } = false;
    
    public Restaurant Restaurant { get; set; }
}