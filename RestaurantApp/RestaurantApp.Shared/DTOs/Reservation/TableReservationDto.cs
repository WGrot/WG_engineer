using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Shared.DTOs.Reservation;

public class TableReservationDto : ReservationDto
{
    public int TableId { get; set; }
    public TableDto Table { get; set; }
}

