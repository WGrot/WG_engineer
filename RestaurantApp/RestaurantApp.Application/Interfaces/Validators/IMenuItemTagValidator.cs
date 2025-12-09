using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemTagValidator
{
    Task<Result> ValidateTagExistsAsync(int tagId, CancellationToken ct = default);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(CreateMenuItemTagDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(int tagId, MenuItemTagDto dto, CancellationToken ct = default);
}