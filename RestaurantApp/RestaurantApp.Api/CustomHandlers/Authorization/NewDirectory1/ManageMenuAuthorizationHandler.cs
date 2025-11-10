using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

public class ManageMenuAuthorizationHandler : AuthorizationHandler<ManageMenuRequirement, int>
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
        ManageMenuRequirement requirement,
        int resource)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {

            var menu = await _context.Menus
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == requirement.MenuId.Value);

            if (menu == null)
            {
                context.Fail();
                return;
            }

            var restaurantId = menu.RestaurantId;

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

            int employeeId = employee.Id;
            // Sprawdź czy użytkownik ma uprawnienie ManageMenu w tej restauracji
            var hasManageMenuPermission = await _context.RestaurantPermissions
                .AsNoTracking()
                .AnyAsync(ep =>
                    ep.RestaurantEmployeeId == employeeId &&
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