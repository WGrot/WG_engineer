using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Services.Validators;

public class MenuValidator : IMenuValidator
{
    private readonly IMenuRepository _menuRepository;
    private readonly IRestaurantRepository _restaurantRepository;

    public MenuValidator(
        IMenuRepository menuRepository,
        IRestaurantRepository restaurantRepository)
    {
        _menuRepository = menuRepository;
        _restaurantRepository = restaurantRepository;
    }

    public async Task<Result> ValidateMenuExistsAsync(int menuId, CancellationToken ct = default)
    {
        var menu = await _menuRepository.GetByIdAsync(menuId, ct);
        if (menu == null)
            return Result.NotFound($"Menu with ID {menuId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateRestaurantExistsAsync(int restaurantId, CancellationToken ct = default)
    {
        var exists = await _restaurantRepository.ExistsAsync(restaurantId, ct);
        if (!exists)
            return Result.NotFound($"Restaurant with ID {restaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateMenuDto dto, CancellationToken ct = default)
    {
        return await ValidateRestaurantExistsAsync(dto.RestaurantId, ct);
    }

    public async Task<Result> ValidateForUpdateAsync(int menuId, CancellationToken ct = default)
    {
        return await ValidateMenuExistsAsync(menuId, ct);
    }

    public async Task<Result> ValidateForDeleteAsync(int menuId, CancellationToken ct = default)
    {
        var menu = await _menuRepository.GetByIdWithDetailsAsync(menuId, ct);
        if (menu == null)
            return Result.NotFound($"Menu with ID {menuId} not found.");

        if (menu.IsActive)
            return Result.Failure("Cannot delete an active menu. Deactivate it first.");

        return Result.Success();
    }
}