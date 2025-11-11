using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Menu;

public class ManageMenuAuthorizationHandler : AuthorizationHandler<ManageMenuRequirement>
{
    private readonly ApiDbContext _context;

    public ManageMenuAuthorizationHandler(
        ApiDbContext context,
        ILogger<ManageMenuAuthorizationHandler> logger)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageMenuRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            int? restaurantId = null;

            // Jeśli mamy MenuId, pobierz RestaurantId z menu
            if (requirement.MenuId.HasValue)
            {
                var menu = await _context.Menus
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.Id == requirement.MenuId.Value);

                if (menu == null)
                {
                    context.Fail();
                    return;
                }

                restaurantId = menu.RestaurantId;
            }
            // Jeśli nie mamy MenuId, użyj RestaurantId z requirement
            else if (requirement.RestaurantId.HasValue)
            {
                restaurantId = requirement.RestaurantId.Value;
            }
            else
            {
                // Brak MenuId i RestaurantId - nie można autoryzować
                context.Fail();
                return;
            }

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

            // Sprawdź czy użytkownik ma uprawnienie ManageMenu w tej restauracji
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
        catch (Exception ex)
        {
            context.Fail();
        }
    }
}