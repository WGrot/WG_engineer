using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs;

public class PaginatedReservationsDto
{
    public List<ReservationDto> Reservations { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasMore { get; set; }
}