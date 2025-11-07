namespace RestaurantApp.Domain.Models;

public class RestaurantSettings
{
    public int Id { get; set; }
    public int RestaurantId { get; set; } 
    public bool ReservationsNeedConfirmation { get; set; } = false;
    
    public Restaurant Restaurant { get; set; }
}