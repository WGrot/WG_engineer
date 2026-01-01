using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IMenuValidator
{
    Task<Result> ValidateMenuExistsAsync(int menuId, CancellationToken ct);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct);
    Task<Result> ValidateForCreateAsync(CreateMenuDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(int menuId, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(int menuId, CancellationToken ct);
}