using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs;

public class PaginatedRestaurantsDto
{
    public IEnumerable<RestaurantDto> Restaurants { get; set; } = new List<RestaurantDto>();
    // Lub jeśli masz RestaurantDto:
    // public IEnumerable<RestaurantDto> Restaurants { get; set; } = new List<RestaurantDto>();
    
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasMore { get; set; }
}