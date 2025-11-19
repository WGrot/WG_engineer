using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Images;
using RestaurantApp.Shared.DTOs.OpeningHours;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantService
{
    Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync();
    Task<Result<RestaurantDto>> GetByIdAsync(int id);
    Task<Result<PaginatedRestaurantsDto>> SearchAsync(string? name, string? address, int page, int pageSize,
        string sortBy);
    Task<Result<IEnumerable<TableDto>>> GetTablesAsync(int restaurantId);
    Task<Result<IEnumerable<RestaurantDto>>> GetOpenNowAsync();
    Task<Result<OpenStatusDto>> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null, DayOfWeek? dayOfWeek = null);
    Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto);
    Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto);
    Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto);
    Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto);
    Task<Result> UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours);
    Task<Result> DeleteAsync(int id);
    
    Task<Result<RestaurantDashboardDataDto>> GetRestaurantDashboardData(int restaurantId);
    Task<Result<List<RestaurantDto>>> GetRestaurantNames(List<int> ids);
}