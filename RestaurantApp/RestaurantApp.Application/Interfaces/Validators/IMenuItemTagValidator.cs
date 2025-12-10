using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IMenuItemTagValidator
{
    Task<Result> ValidateTagExistsAsync(int tagId);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateForCreateAsync(CreateMenuItemTagDto dto);
    Task<Result> ValidateForUpdateAsync(int tagId, MenuItemTagDto dto);
}