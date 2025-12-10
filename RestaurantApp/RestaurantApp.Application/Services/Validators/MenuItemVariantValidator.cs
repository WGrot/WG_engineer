using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Services.Validators;

public class MenuItemVariantValidator : IMenuItemVariantValidator
{
    private readonly IMenuItemVariantRepository _repository;

    public MenuItemVariantValidator(IMenuItemVariantRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result> ValidateVariantExistsAsync(int variantId, CancellationToken ct = default)
    {
        var variant = await _repository.GetByIdAsync(variantId);
        if (variant == null)
            return Result.NotFound($"MenuItem variant with ID {variantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateMenuItemExistsAsync(int menuItemId, CancellationToken ct = default)
    {
        var exists = await _repository.MenuItemExistsAsync(menuItemId);
        if (!exists)
            return Result.NotFound($"MenuItem with ID {menuItemId} does not exist.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(MenuItemVariantDto dto, CancellationToken ct = default)
    {
        return await ValidateMenuItemExistsAsync(dto.MenuItemId, ct);
    }

    public async Task<Result> ValidateForUpdateAsync(int variantId, MenuItemVariantDto dto, CancellationToken ct = default)
    {
        return await ValidateVariantExistsAsync(variantId, ct);
    }

    public async Task<Result> ValidateForDeleteAsync(int variantId, CancellationToken ct = default)
    {
        return await ValidateVariantExistsAsync(variantId, ct);
    }
}