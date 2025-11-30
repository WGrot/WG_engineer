namespace RestaurantApp.Domain.Models;

public class TableReservation: ReservationBase
{
    public int TableId { get; set; }
    public Table Table { get; set; } = null!;
}