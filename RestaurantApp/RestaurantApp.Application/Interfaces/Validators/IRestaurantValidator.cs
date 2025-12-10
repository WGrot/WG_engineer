using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IRestaurantValidator
{
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateUniqueNameAndAddressAsync(string name, string address, int? excludeId = null);
    Task<Result> ValidateForCreateAsync(RestaurantDto dto);
    Task<Result> ValidateForCreateAsUserAsync(CreateRestaurantDto dto);
    Task<Result> ValidateForUpdateAsync(int id, RestaurantDto dto);
}