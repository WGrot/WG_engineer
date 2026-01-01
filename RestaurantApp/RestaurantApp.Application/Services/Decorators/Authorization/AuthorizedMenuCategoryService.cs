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
        if (!await AuthorizeForMenuAsync(categoryDto.MenuId, ct))
            return Result<MenuCategoryDto>.Forbidden("You dont have permission to create categories for this restaurant.");
        
        return await _inner.CreateCategoryAsync(categoryDto, ct);
    }

    public async Task<Result> UpdateCategoryAsync(UpdateMenuCategoryDto categoryDto, CancellationToken ct)
    {
        if (!await AuthorizeForCategoryAsync(categoryDto.Id, ct))
            return Result.Forbidden("You don't have permission to manage this category");

        return await _inner.UpdateCategoryAsync(categoryDto, ct);
    }

    public async Task<Result> DeleteCategoryAsync(int categoryId, CancellationToken ct)
    {
        if (!await AuthorizeForCategoryAsync(categoryId, ct))
            return Result.Forbidden("You don't have permission to manage this category");

        return await _inner.DeleteCategoryAsync(categoryId, ct);
    }

    public async Task<Result> UpdateCategoryOrderAsync(int categoryId, int displayOrder, CancellationToken ct)
    {
        if (!await AuthorizeForCategoryAsync(categoryId, ct))
            return Result.Forbidden("You don't have permission to manage this category");

        return await _inner.DeleteCategoryAsync(categoryId, ct);
    }
    
    private async Task<bool> AuthorizeForCategoryAsync(int categoryId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageCategoryAsync(_currentUser.UserId!, categoryId);
    }
    
    private async Task<bool> AuthorizeForMenuAsync(int menuId, CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return false;

        return await _authorizationChecker.CanManageMenuByMenuIdAsync(_currentUser.UserId!, menuId);
    }
}