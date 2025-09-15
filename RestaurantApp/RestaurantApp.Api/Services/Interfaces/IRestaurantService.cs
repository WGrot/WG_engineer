using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IRestaurantService
{
    Task<IEnumerable<Restaurant>> GetAllAsync();
    Task<Restaurant?> GetByIdAsync(int id);
    Task<IEnumerable<Restaurant>> SearchAsync(string? name, string? address);
    Task<IEnumerable<Table>> GetTablesAsync(int restaurantId);
    Task<IEnumerable<Restaurant>> GetOpenNowAsync();
    Task<OpenStatusDto> CheckIfOpenAsync(int restaurantId, TimeOnly? time = null, DayOfWeek? dayOfWeek = null);
    Task<Restaurant> CreateAsync(RestaurantDto restaurantDto);
    Task UpdateAsync(int id, RestaurantDto restaurantDto);
    Task UpdateAddressAsync(int id, string address);
    Task UpdateNameAsync(int id, string name);
    Task UpdateOpeningHoursAsync(int id, List<OpeningHoursDto> openingHours);
    Task DeleteAsync(int id);
}