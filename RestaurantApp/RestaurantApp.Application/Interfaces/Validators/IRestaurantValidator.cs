using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IRestaurantValidator
{
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateUniqueNameAndAddressAsync(string name, string address, int? excludeId = null, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(RestaurantDto dto, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsUserAsync(CreateRestaurantDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(int id, RestaurantDto dto, CancellationToken ct = default);
}