namespace RestaurantApp.Application.Interfaces;

public interface IAuthorizationChecker
{
    Task<bool> CanManageCategoryAsync(string userId, int categoryId);
    Task<bool> CanManageMenuAsync(string userId, int restaurantId);
    
    Task<bool> CanManageMenuByMenuIdAsync(string userId, int menuId);
    
}