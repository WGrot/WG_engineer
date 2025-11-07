namespace RestaurantApp.Shared.DTOs.Restaurant;

public class RestaurantDashboardDataDto
{
    public int TodayReservations { get; set; }
    public int AvailableTables { get; set; }
    public int AvailableSeats { get; set; }
    public int ReservationsLastWeek { get; set; }
}