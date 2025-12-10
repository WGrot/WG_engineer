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

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetAllVariantsAsync()
    {
        return await _inner.GetAllVariantsAsync();
    }

    public async Task<Result<IEnumerable<MenuItemVariantDto>>> GetMenuItemVariantsAsync(int menuItemId)
    {
        return await _inner.GetMenuItemVariantsAsync(menuItemId);
    }

    public async Task<Result<MenuItemVariantDto>> GetVariantByIdAsync(int id)
    {
        return await _inner.GetVariantByIdAsync(id);
    }

    public async Task<Result<MenuItemVariantDto>> CreateVariantAsync(MenuItemVariantDto variantDto)
    {
        var fluentResult = await _validator.ValidateAsync(variantDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuItemVariantDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(variantDto);
        if (!businessResult.IsSuccess)
            return Result<MenuItemVariantDto>.From(businessResult);

        return await _inner.CreateVariantAsync(variantDto);
    }

    public async Task<Result<MenuItemVariantDto>> UpdateVariantAsync(int id, MenuItemVariantDto variantDto)
    {
        var fluentResult = await _validator.ValidateAsync(variantDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuItemVariantDto>();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(id, variantDto);
        if (!businessResult.IsSuccess)
            return Result<MenuItemVariantDto>.From(businessResult);

        return await _inner.UpdateVariantAsync(id, variantDto);
    }

    public async Task<Result> DeleteVariantAsync(int id)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteVariantAsync(id);
    }
}