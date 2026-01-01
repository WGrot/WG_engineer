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

    public async Task<Result<IEnumerable<RestaurantDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<RestaurantDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<RestaurantDto>> CreateAsync(RestaurantDto restaurantDto, CancellationToken ct)
    {
        return await _inner.CreateAsync(restaurantDto, ct);
    }

    public async Task<Result<RestaurantDto>> CreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct)
    {
        return await _inner.CreateAsUserAsync(dto, ct);
    }

    public async Task<Result> UpdateAsync(int id, RestaurantDto restaurantDto, CancellationToken ct)
    {
        return await _inner.UpdateAsync(id, restaurantDto, ct);
    }

    public async Task<Result> UpdateBasicInfoAsync(int id, RestaurantBasicInfoDto dto, CancellationToken ct)
    {
        return await _inner.UpdateBasicInfoAsync(id, dto, ct);
    }

    public async Task<Result> UpdateStructuredAddressAsync(int id, StructuresAddressDto dto, CancellationToken ct)
    {
        return await _inner.UpdateStructuredAddressAsync(id, dto, ct);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        return await _inner.DeleteAsync(id, ct);
    }

    public async Task<Result<List<RestaurantDto>>> GetRestaurantNamesAsync(List<int> ids, CancellationToken ct)
    {
        return await _inner.GetRestaurantNamesAsync(ids, ct);
    }
}