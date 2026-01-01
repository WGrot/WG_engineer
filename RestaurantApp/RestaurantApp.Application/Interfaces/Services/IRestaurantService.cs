using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IRestaurantService
{
    Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<RestaurantDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto, CancellationToken ct = default);
    Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct = default);
    Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto, CancellationToken ct = default);
    Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto, CancellationToken ct = default);
    Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids, CancellationToken ct = default);
}