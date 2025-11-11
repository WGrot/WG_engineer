using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuCategory;

public class ManageCategoryAuthorizationHandler: AuthorizationHandler<ManageCategoryRequirement>
{
    private readonly ApiDbContext _context;

    public ManageCategoryAuthorizationHandler(ApiDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageCategoryRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            // Pobierz kategorię wraz z Menu i Restaurant
            var category = await _context.MenuCategories
                .AsNoTracking()
                .Include(c => c.Menu)
                .FirstOrDefaultAsync(c => c.Id == requirement.CategoryId);

            if (category?.Menu == null)
            {
                context.Fail();
                return;
            }

            var restaurantId = category.Menu.RestaurantId;

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