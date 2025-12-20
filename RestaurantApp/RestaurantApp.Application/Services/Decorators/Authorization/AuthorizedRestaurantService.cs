using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.GeoCoding;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedRestaurantService : IRestaurantService
{
    private readonly IRestaurantService _inner;

    public AuthorizedRestaurantService(
        IRestaurantService inner)
    {
        _inner = inner;

    }

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id)
    {
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto)
    {
        return await _inner.CreateAsync(restaurantDto);
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto)
    {
        return await _inner.CreateAsUserAsync(dto);
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto)
    {
        return await _inner.UpdateAsync(id, restaurantDto);
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto)
    {
        return await _inner.UpdateBasicInfoAsync(id, dto);
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto)
    {
        return await _inner.UpdateStructuredAddressAsync(id, dto);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        return await _inner.DeleteAsync(id);
    }

    public async Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids)
    {
        return await _inner.GetRestaurantNamesAsync(ids);
    }
}