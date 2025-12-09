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

    public async Task<Result<MenuCategoryDto>> GetCategoryByIdAsync(int categoryId)
    {
        return await _inner.GetCategoryByIdAsync(categoryId);
    }

    public async Task<Result<IEnumerable<MenuCategoryDto>>> GetCategoriesAsync(int? menuId)
    {
        return await _inner.GetCategoriesAsync(menuId);
    }

    public async Task<Result<MenuCategoryDto>> CreateCategoryAsync(CreateMenuCategoryDto categoryDto)
    {
        var fluentResult = await _createValidator.ValidateAsync(categoryDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<MenuCategoryDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(categoryDto);
        if (!businessResult.IsSuccess)
            return Result<MenuCategoryDto>.From(businessResult);

        return await _inner.CreateCategoryAsync(categoryDto);
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto)
    {
        var fluentResult = await _updateValidator.ValidateAsync(categoryDto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(categoryDto);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateCategoryAsync(categoryDto);
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        var businessResult = await _businessValidator.ValidateCategoryExistsAsync(categoryId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteCategoryAsync(categoryId);
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder)
    {
        var businessResult = await _businessValidator.ValidateCategoryExistsAsync(categoryId);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateCategoryOrderAsync(categoryId, displayOrder);
    }
}