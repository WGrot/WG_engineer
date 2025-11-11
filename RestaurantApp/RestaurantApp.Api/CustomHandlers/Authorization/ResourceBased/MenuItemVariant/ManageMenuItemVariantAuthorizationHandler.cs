using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItemVariant;

public class ManageMenuItemVariantAuthorizationHandler: AuthorizationHandler<ManageMenuItemVariantRequirement>
{
    private readonly ApiDbContext _context;

    public ManageMenuItemVariantAuthorizationHandler(ApiDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageMenuItemVariantRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {

            var variant = await _context.MenuItemVariants
                .AsNoTracking()
                .Include(v => v.MenuItem)           
                    .ThenInclude(mi => mi.Menu)     
                .FirstOrDefaultAsync(v => v.Id == requirement.MenuItemVariantId);

            if (variant?.MenuItem?.Menu == null)
            {
                context.Fail();
                return;
            }

            var restaurantId = variant.MenuItem.Menu.RestaurantId;
            
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