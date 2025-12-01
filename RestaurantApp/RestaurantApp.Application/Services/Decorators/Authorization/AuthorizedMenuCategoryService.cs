using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Menu.Categories;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedMenuCategoryService: IMenuCategoryService
{
    private readonly IMenuCategoryService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedMenuCategoryService(
        IMenuCategoryService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
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
        if (!await AuthorizeForMenuAsync(categoryDto.MenuId))
            return Result<MenuCategoryDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateCategoryAsync(categoryDto);
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto)
    {
        if (!await AuthorizeForCategoryAsync(categoryDto.Id))
            return Result.Forbidden("You don't have permission to manage this category");

        return await _inner.UpdateCategoryAsync(categoryDto);
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId)
    {
        if (!await AuthorizeForCategoryAsync(categoryId))
            return Result.Forbidden("You don't have permission to manage this category");

        return await _inner.DeleteCategoryAsync(categoryId);
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder)
    {
        if (!await AuthorizeForCategoryAsync(categoryId))
            return Result.Forbidden("You don't have permission to manage this category");

        return await _inner.DeleteCategoryAsync(categoryId);
    }
    
    private async Task<bool> AuthorizeForCategoryAsync(int categoryId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageCategoryAsync(_currentUser.UserId!, categoryId);
    }
    
    private async Task<bool> AuthorizeForMenuAsync(int menuId)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuByMenuIdAsync(_currentUser.UserId!, menuId);
    }
}