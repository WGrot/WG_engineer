using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Tags;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedMenuItemTagService : IMenuItemTagService
{
    private readonly IMenuItemTagService _inner;
    private readonly IMenuItemTagValidator _businessValidator;
    private readonly IValidator<CreateMenuItemTagDto> _createValidator;

    public ValidatedMenuItemTagService(
        IMenuItemTagService inner,
        IValidator<CreateMenuItemTagDto> createValidator,
        IMenuItemTagValidator businessValidator)
    {
        _inner = inner;
        _businessValidator = businessValidator;
        _createValidator = createValidator;
    }

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(int? restaurantId = null)
    {
        return await _inner.GetTagsAsync(restaurantId);
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id)
    {
        var validationResult = await _businessValidator.ValidateTagExistsAsync(id);
        if (!validationResult.IsSuccess)
            return Result<MenuItemTagDto?>.From(validationResult);

        return await _inner.GetTagByIdAsync(id);
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto dto)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuItemTagDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto);
        if (!businessResult.IsSuccess)
            return Result<MenuItemTagDto>.From(businessResult);

        return await _inner.CreateTagAsync(dto);
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto dto)
    {
        var validationResult = await _businessValidator.ValidateForUpdateAsync(id, dto);
        if (!validationResult.IsSuccess)
            return Result<MenuItemTagDto>.From(validationResult);

        return await _inner.UpdateTagAsync(id, dto);
    }

    public async Task<Result> DeleteTagAsync(int id)
    {
        var validationResult = await _businessValidator.ValidateTagExistsAsync(id);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteTagAsync(id);
    }

    public async Task<Result<bool>> TagExistsAsync(int id)
    {
        return await _inner.TagExistsAsync(id);
    }
}