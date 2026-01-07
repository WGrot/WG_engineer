using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Infrastructure.Persistence;

namespace RestaurantApp.Infrastructure.Services;

public class AuthorizationChecker: IAuthorizationChecker
{
    private readonly ApplicationDbContext _context;

    public AuthorizationChecker(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanManageCategoryAsync(string userId, int categoryId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Where(e => e.Restaurant.Menu != null &&
                        e.Restaurant.Menu.Categories.Any(c => c.Id == categoryId))
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageMenu);
    }

    public async Task<bool> CanManageMenuAsync(string userId, int restaurantId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive && e.RestaurantId == restaurantId)
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageMenu);
    }

    public async Task<bool> CanManageMenuByMenuIdAsync(string userId, int menuId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Where(e => e.Restaurant.Menu != null && e.Restaurant.Menu.Id == menuId)
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageMenu);
    }

    public async Task<bool> CanManageMenuItemAsync(string userId, int menuItemId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Where(e => e.Restaurant.Menu != null &&
                        e.Restaurant.Menu.Items.Any(i => i.Id == menuItemId))
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageMenu);
    }

    public async Task<bool> CanManageTagAsync(string userId, int tagId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Where(e => e.Restaurant.MenuItemTags.Any(t => t.Id == tagId))
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageMenu);
    }

    public async Task<bool> CanManageMenuItemVariantAsync(string userId, int menuItemVariantId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Where(e => e.Restaurant.Menu != null &&
                        e.Restaurant.Menu.Items.Any(i => i.Variants.Any(v => v.Id == menuItemVariantId)))
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageMenu);
    }

    public async Task<bool> CanManagePermissionAsync(string userId, int permissionId)
    {
        return await _context.RestaurantPermissions
            .AsNoTracking()
            .Where(p => p.Id == permissionId)
            .Where(p => p.RestaurantEmployee.RestaurantId != null)
            .Join(
                _context.RestaurantEmployees.AsNoTracking()
                    .Where(e => e.UserId == userId && e.IsActive),
                targetPermission => targetPermission.RestaurantEmployee.RestaurantId,
                currentEmployee => currentEmployee.RestaurantId,
                (targetPermission, currentEmployee) => currentEmployee
            )
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManagePermissions);
    }

    public async Task<bool> HasPermissionInRestaurantAsync(string userId, int restaurantId, PermissionType permission)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive && e.RestaurantId == restaurantId)
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == permission);
    }

    public async Task<bool> CanManageEmployeePermissionsAsync(string userId, int restaurantEmployeeId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.Id == restaurantEmployeeId)
            .Join(
                _context.RestaurantEmployees.AsNoTracking()
                    .Where(e => e.UserId == userId && e.IsActive),
                targetEmployee => targetEmployee.RestaurantId,
                currentEmployee => currentEmployee.RestaurantId,
                (targetEmployee, currentEmployee) => currentEmployee
            )
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManagePermissions);
    }

    public async Task<bool> CanManageTableAsync(string userId, int tableId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .Where(e => e.Restaurant.Tables.Any(t => t.Id == tableId))
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageTables);
    }

    public async Task<bool> CanManageReservationAsync(string userId, int reservationId, bool needToBeEmployee = true)
    {
        var reservation = await _context.Reservations
            .AsNoTracking()
            .Where(r => r.Id == reservationId)
            .Select(r => new { r.UserId, r.RestaurantId })
            .FirstOrDefaultAsync();

        if (reservation == null)
            return false;

        if (!needToBeEmployee && reservation.UserId == userId)
            return true;

        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive && e.RestaurantId == reservation.RestaurantId)
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageReservations);
    }

    public async Task<bool> CanManageEmployeeAsync(string userId, int restaurantEmployeeId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.Id == restaurantEmployeeId)
            .Join(
                _context.RestaurantEmployees.AsNoTracking()
                    .Where(e => e.UserId == userId && e.IsActive),
                targetEmployee => targetEmployee.RestaurantId,
                currentEmployee => currentEmployee.RestaurantId,
                (targetEmployee, currentEmployee) => currentEmployee
            )
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageEmployees);
    }

    public async Task<bool> IsEmployeeWithId(string userId, int restaurantEmployeeId)
    {
        return await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.Id == restaurantEmployeeId && e.UserId == userId)
            .AnyAsync();
    }

    public async Task<bool> HasPermissionInAnyRestaurantAsync(string userId, PermissionType permission)
    {
        var result = await _context.RestaurantEmployees
            .AsNoTracking()
            .Where(e => e.UserId == userId && e.IsActive)
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == permission);

        return result;
    }
    

    public async Task<bool> CanManageRestaurantSettingsAsync(string userId, int restaurantSettingsId)
    {
        return await _context.RestaurantSettings
            .AsNoTracking()
            .Where(s => s.Id == restaurantSettingsId)
            .Join(
                _context.RestaurantEmployees.AsNoTracking()
                    .Where(e => e.UserId == userId && e.IsActive),
                settings => settings.RestaurantId,
                employee => employee.RestaurantId,
                (settings, employee) => employee
            )
            .SelectMany(e => e.Permissions)
            .AnyAsync(p => p.Permission == PermissionType.ManageRestaurantSettings);
    }
}