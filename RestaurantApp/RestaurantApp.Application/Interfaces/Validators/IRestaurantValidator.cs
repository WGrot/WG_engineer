using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IRestaurantValidator
{
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct);
    Task<Result> ValidateUniqueNameAndAddressAsync(string name, string address, CancellationToken ct, int? excludeId = null);
    Task<Result> ValidateForCreateAsync(RestaurantDto dto, CancellationToken ct);
    Task<Result> ValidateForCreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int id, RestaurantDto dto, CancellationToken ct);
}