using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedMenuCategoryService: IMenuCategoryService
{
    private readonly IMenuCategoryService _inner;
    private readonly IValidator<CreateMenuCategoryDto> _createValidator;
    private readonly IValidator<UpdateMenuCategoryDto> _updateValidator;
    private readonly IMenuCategoryValidator _businessValidator;

    public ValidatedMenuCategoryService(
        IMenuCategoryService inner,
        IValidator<CreateMenuCategoryDto> createValidator,
        IValidator<UpdateMenuCategoryDto> updateValidator,
        IMenuCategoryValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId, CancellationToken ct)
    {
        return await _inner.GetCategoryByIdAsync(categoryId, ct);
    }

    public async Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId, CancellationToken ct)
    {
        return await _inner.GetCategoriesAsync(menuId, ct);
    }

    public async Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        var fluentResult = await _createValidator.ValidateAsync(categoryDto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuCategoryDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(categoryDto, ct);
        if (!businessResult.IsSuccess)
            return Result<MenuCategoryDto>.From(businessResult);

        return await _inner.CreateCategoryAsync(categoryDto, ct);
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        var fluentResult = await _updateValidator.ValidateAsync(categoryDto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(categoryDto, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateCategoryAsync(categoryDto, ct);
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateCategoryExistsAsync(categoryId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteCategoryAsync(categoryId, ct);
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder, CancellationToken ct)
    {
        var businessResult = await _businessValidator.ValidateCategoryExistsAsync(categoryId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateCategoryOrderAsync(categoryId, displayOrder, ct);
    }
}