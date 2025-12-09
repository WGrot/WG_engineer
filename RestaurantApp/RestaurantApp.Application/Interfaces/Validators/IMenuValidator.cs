using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IMenuValidator
{
    Task<Result> ValidateMenuExistsAsync(int menuId, CancellationToken ct = default);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(CreateMenuDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(int menuId, CancellationToken ct = default);
    Task<Result> ValidateForDeleteAsync(int menuId, CancellationToken ct = default);
}