using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Services.Validators;

public class RestaurantValidator: IRestaurantValidator
{
    private readonly IRestaurantRepository _restaurantRepository;

    public RestaurantValidator(IRestaurantRepository restaurantRepository)
    {
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId, ct);
        if (!exists)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateUniqueNameAndAddressAsync(string name,
        string address, CancellationToken ct,
        int? excludeId = null)
    {
        var exists = await _restaurantRepository.ExistsWithNameAndAddressAsync(name, address, ct, excludeId);
        if (exists)
            return Result.Failure("A restaurant with the same name and address already exists.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(RestaurantDto dto, CancellationToken ct)
    {
        return await ValidateUniqueNameAndAddressAsync(dto.Name, dto.Address, ct);
    }

    public async Task<Result> ValidateForCreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct)
    {
        return await ValidateUniqueNameAndAddressAsync(dto.Name, dto.Address, ct);
    }

    public async Task<Result> ValidateForUpdateAsync(int id, RestaurantDto dto, CancellationToken ct)
    {
        var existsResult = await ValidateRestaurantExistsAsync(id, ct);
        if (!existsResult.IsSuccess)
            return existsResult;

        return await ValidateUniqueNameAndAddressAsync(dto.Name, dto.Address, ct, excludeId: id);
    }
}