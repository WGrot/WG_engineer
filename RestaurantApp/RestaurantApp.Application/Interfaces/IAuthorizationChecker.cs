namespace RestaurantApp.Application.Interfaces;

public interface IAuthorizationChecker
{
    Task<bool> CanManageCategoryAsync(string userId, int categoryId);
    Task<bool> CanManageMenuAsync(string userId, int restaurantId);
    
    Task<bool> CanManageMenuByMenuIdAsync(string userId, int menuId);
    
    Task<bool> CanManageMenuItemAsync(string userId, int menuItemId);
    
    Task<bool> CanManageTagAsync(string userId, int tagId);
    
    Task<bool> CanManageMenuItemVariantAsync(string userId, int menuItemVariantId);
}