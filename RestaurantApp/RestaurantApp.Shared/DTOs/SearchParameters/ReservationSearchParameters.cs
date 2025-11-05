using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.SearchParameters;

public class ReservationSearchParameters
{
    public int? RestaurantId { get; set; }
    
    public string? RestaurantName { get; set; }
    public string? UserId { get; set; }
    public ReservationStatus? Status { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerEmail { get; set; }
    public string? CustomerPhone { get; set; }
    public DateTime? ReservationDate { get; set; }
    public DateTime? ReservationDateFrom { get; set; }
    public DateTime? ReservationDateTo { get; set; }
    public string? Notes { get; set; }

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 5;
    public string SortBy { get; set; } = "newest";
}