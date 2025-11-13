using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuCategory;

public class ManageCategoryAuthorizationHandler : AuthorizationHandler<ManageCategoryRequirement>
{
    private readonly ApiDbContext _context;
    private readonly ILogger<ManageCategoryAuthorizationHandler> _logger;

    public ManageCategoryAuthorizationHandler(ApiDbContext context, ILogger<ManageCategoryAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageCategoryRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            // składamy zapytanie bazowe po pracownikach użytkownika
            var query = _context.RestaurantEmployees
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.IsActive);

            // teraz zawężamy po kategorii — szukamy pracownika, który pracuje
            // w restauracji posiadającej menu z daną kategorią
            query = query.Where(e =>
                e.Restaurant.Menu != null &&
                e.Restaurant.Menu.Categories.Any(c => c.Id == requirement.CategoryId));

            // sprawdzamy czy ma uprawnienie ManageMenu
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
            _logger.LogError(ex, "Authorization error in ManageCategoryAuthorizationHandler");
            context.Fail();
        }
    }
}