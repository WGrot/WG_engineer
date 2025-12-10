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

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId);
        if (!exists)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateUniqueNameAndAddressAsync(string name,
        string address,
        int? excludeId = null)
    {
        var exists = await _restaurantRepository.ExistsWithNameAndAddressAsync(name, address, excludeId);
        if (exists)
            return Result.Failure("A restaurant with the same name and address already exists.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(RestaurantDto dto)
    {
        return await ValidateUniqueNameAndAddressAsync(dto.Name, dto.Address);
    }

    public async Task<Result> ValidateForCreateAsUserAsync(CreateRestaurantDto dto)
    {
        return await ValidateUniqueNameAndAddressAsync(dto.Name, dto.Address);
    }

    public async Task<Result> ValidateForUpdateAsync(int id, RestaurantDto dto)
    {
        var existsResult = await ValidateRestaurantExistsAsync(id);
        if (!existsResult.IsSuccess)
            return existsResult;

        return await ValidateUniqueNameAndAddressAsync(dto.Name, dto.Address, excludeId: id);
    }
}