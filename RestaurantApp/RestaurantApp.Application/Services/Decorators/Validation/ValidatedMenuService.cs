using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedMenuService : IMenuService
{
    private readonly IMenuService _inner;
    private readonly IValidator<CreateMenuDto> _createValidator;
    private readonly IValidator<UpdateMenuDto> _updateValidator;
    private readonly IMenuValidator _businessValidator;

    public ValidatedMenuService(
        IMenuService inner,
        IValidator<CreateMenuDto> createValidator,
        IValidator<UpdateMenuDto> updateValidator,
        IMenuValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<MenuDto>> GetMenuByIdAsync(int menuId)
    {
        return await _inner.GetMenuByIdAsync(menuId);
    }

    public async Task<Result<MenuDto>> GetMenusAsync(int restaurantId, bool? isActive = null)
    {
        return await _inner.GetMenusAsync(restaurantId, isActive);
    }

    public async Task<Result<MenuDto>> GetActiveMenuByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetActiveMenuByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<MenuDto>> CreateMenuAsync(CreateMenuDto dto)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<MenuDto>.From(businessResult);

        return await _inner.CreateMenuAsync(dto);
    }

    public async Task<Result> UpdateMenuAsync(int menuId, UpdateMenuDto dto)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(menuId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateMenuAsync(menuId, dto);
    }

    public async Task<Result> DeleteMenuAsync(int menuId)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(menuId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteMenuAsync(menuId);
    }

    public async Task<Result> ActivateMenuAsync(int menuId)
    {
        var businessResult = await _businessValidator.ValidateMenuExistsAsync(menuId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.ActivateMenuAsync(menuId);
    }

    public async Task<Result> DeactivateMenuAsync(int menuId)
    {
        var businessResult = await _businessValidator.ValidateMenuExistsAsync(menuId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeactivateMenuAsync(menuId);
    }
}