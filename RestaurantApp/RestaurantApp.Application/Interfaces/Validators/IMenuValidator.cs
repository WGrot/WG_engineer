using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IMenuValidator
{
    Task<Result> ValidateMenuExistsAsync(int menuId);
    Task<Result> ValidateRestaurantExistsAsync(int restaurantId);
    Task<Result> ValidateForCreateAsync(CreateMenuDto dto);
    Task<Result> ValidateForUpdateAsync(int menuId);
    Task<Result> ValidateForDeleteAsync(int menuId);
}