using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItem;

public class ManageMenuItemAuthorizationHandler : AuthorizationHandler<ManageMenuItemRequirement>
{
    private readonly ApiDbContext _context;

    public ManageMenuItemAuthorizationHandler(ApiDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageMenuItemRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            var item = await _context.MenuItems
                .AsNoTracking()
                .Include(i => i.Menu)
                .FirstOrDefaultAsync(i => i.Id == requirement.MenuItemId);

            if (item?.Menu == null)
            {
                context.Fail();
                return;
            }

            var restaurantId = item.Menu.RestaurantId;

            // Sprawdź czy użytkownik pracuje w tej restauracji
            var employee = await _context.RestaurantEmployees
                .AsNoTracking()
                .FirstOrDefaultAsync(er =>
                    er.UserId == userId &&
                    er.RestaurantId == restaurantId &&
                    er.IsActive);

            if (employee == null)
            {
                context.Fail();
                return;
            }

            // Sprawdź uprawnienie ManageMenu
            var hasManageMenuPermission = await _context.RestaurantPermissions
                .AsNoTracking()
                .AnyAsync(ep =>
                    ep.RestaurantEmployeeId == employee.Id &&
                    ep.Permission == PermissionType.ManageMenu);

            if (hasManageMenuPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        catch (Exception)
        {
            context.Fail();
        }
    }
}