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

    public async Task<Result<IEnumerable<MenuItemTagDto>>> GetTagsAsync(CancellationToken ct, int? restaurantId = null)
    {
        return await _inner.GetTagsAsync(ct, restaurantId);
    }

    public async Task<Result<MenuItemTagDto?>> GetTagByIdAsync(int id, CancellationToken ct)
    {
        var validationResult = await _businessValidator.ValidateTagExistsAsync(id, ct);
        if (!validationResult.IsSuccess)
            return Result<MenuItemTagDto?>.From(validationResult);

        return await _inner.GetTagByIdAsync(id, ct);
    }

    public async Task<Result<MenuItemTagDto>> CreateTagAsync(CreateMenuItemTagDto dto, CancellationToken ct)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuItemTagDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return Result<MenuItemTagDto>.From(businessResult);

        return await _inner.CreateTagAsync(dto, ct);
    }

    public async Task<Result<MenuItemTagDto>> UpdateTagAsync(int id, MenuItemTagDto dto, CancellationToken ct)
    {
        var validationResult = await _businessValidator.ValidateForUpdateAsync(id, dto, ct);
        if (!validationResult.IsSuccess)
            return Result<MenuItemTagDto>.From(validationResult);

        return await _inner.UpdateTagAsync(id, dto, ct);
    }

    public async Task<Result> DeleteTagAsync(int id, CancellationToken ct)
    {
        var validationResult = await _businessValidator.ValidateTagExistsAsync(id, ct);
        if (!validationResult.IsSuccess)
            return validationResult;

        return await _inner.DeleteTagAsync(id, ct);
    }

    public async Task<Result<bool>> TagExistsAsync(int id, CancellationToken ct)
    {
        return await _inner.TagExistsAsync(id, ct);
    }
}