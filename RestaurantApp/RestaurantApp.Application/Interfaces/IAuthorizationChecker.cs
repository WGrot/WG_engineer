using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Application.Interfaces;

public interface IAuthorizationChecker
{
    Task<bool> CanManageCategoryAsync(string userId, int categoryId);
    Task<bool> CanManageMenuAsync(string userId, int restaurantId);
    
    Task<bool> CanManageMenuByMenuIdAsync(string userId, int menuId);
    
    Task<bool> CanManageMenuItemAsync(string userId, int menuItemId);
    
    Task<bool> CanManageTagAsync(string userId, int tagId);
    
    Task<bool> CanManageMenuItemVariantAsync(string userId, int menuItemVariantId);
    
    Task<bool> CanManagePermissionAsync(string userId, int permissionId);
    
    Task<bool> HasPermissionInRestaurantAsync(string userId, int restaurantId, PermissionType permission);
    
    Task<bool> CanManageEmployeePermissionsAsync(string userId, int restaurantEmployeeId);
    Task<bool> CanManageTableAsync(string userId, int tableId);
    Task<bool> CanManageTablesInRestaurantAsync(string userId, int restaurantId);
    
    Task<bool> CanManageReservationAsync(string userId, int reservationId, bool needToBeEmployee = true);
}