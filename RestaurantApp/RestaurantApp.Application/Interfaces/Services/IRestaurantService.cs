using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantService
{
    Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync();
    Task<Result<RestaurantDto>> GetByIdAsync(int id);
    Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto);
    Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto);
    Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto);
    Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto);
    Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids);
}