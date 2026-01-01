using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Variants;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedMenuItemVariantService : IMenuItemVariantService
{
    private readonly IMenuItemVariantService _inner;
    private readonly IValidator<MenuItemVariantDto> _validator;
    private readonly IMenuItemVariantValidator _businessValidator;

    public ValidatedMenuItemVariantService(
        IMenuItemVariantService inner,
        IValidator<MenuItemVariantDto> validator,
        IMenuItemVariantValidator businessValidator)
    {
        _inner = inner;
        _validator = validator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync(CancellationToken ct)
    {
        return await _inner.GetAllVariantsAsync(ct);
    }
    

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId, CancellationToken ct)
    {
        return await _inner.GetMenuItemVariantsAsync(menuItemId, ct);
    }

    public async Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetVariantByIdAsync(id, ct);
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variantDto, CancellationToken ct)
    {
        var fluentResult = await _validator.ValidateAsync(variantDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuItemVariantDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(variantDto, ct);
        if (!businessResult.IsSuccess)
            return Result<MenuItemVariantDto>.From(businessResult);

        return await _inner.CreateVariantAsync(variantDto, ct);
    }

    public async Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto, CancellationToken ct)
    {
        var fluentResult = await _validator.ValidateAsync(variantDto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuItemVariantDto>();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, variantDto, ct);
        if (!businessResult.IsSuccess)
            return Result<MenuItemVariantDto>.From(businessResult);

        return await _inner.UpdateVariantAsync(id, variantDto, ct);
    }

    public async Task<Result> DeleteVariantAsync(int id, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(id, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteVariantAsync(id, ct);
    }
}