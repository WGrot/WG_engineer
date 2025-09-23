using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api;

public class ReservationSearchParameters
{
    public int? RestaurantId { get; set; }
    public string? UserId { get; set; }
    public ReservationStatus? Status { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? ReservationDate { get; set; }
    public DateTime? ReservationDateFrom { get; set; }
    public DateTime? ReservationDateTo { get; set; }
    public string? Notes { get; set; }
}