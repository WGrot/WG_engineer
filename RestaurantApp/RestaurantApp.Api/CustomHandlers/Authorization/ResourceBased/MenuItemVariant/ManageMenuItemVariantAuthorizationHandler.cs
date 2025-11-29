using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItemVariant;

public class ManageMenuItemVariantAuthorizationHandler : AuthorizationHandler<ManageMenuItemVariantRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManageMenuItemVariantAuthorizationHandler> _logger;

    public ManageMenuItemVariantAuthorizationHandler(ApplicationDbContext context, ILogger<ManageMenuItemVariantAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageMenuItemVariantRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            // bazowe zapytanie – aktywni pracownicy użytkownika
            var query = _context.RestaurantEmployees
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.IsActive);

            // filtr po wariancie
            query = query.Where(e =>
                e.Restaurant.Menu != null &&
                e.Restaurant.Menu.Items
                    .Any(i => i.Variants.Any(v => v.Id == requirement.MenuItemVariantId)));

            // sprawdzenie uprawnienia ManageMenu
            var hasPermission = await query
                .SelectMany(e => e.Permissions)
                .AnyAsync(p => p.Permission == PermissionType.ManageMenu);

            if (hasPermission)
                context.Succeed(requirement);
            else
                context.Fail();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization error in ManageMenuItemVariantAuthorizationHandler");
            context.Fail();
        }
    }
}