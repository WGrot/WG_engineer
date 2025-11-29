using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Menu;

public class ManageMenuAuthorizationHandler : AuthorizationHandler<ManageMenuRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManageMenuAuthorizationHandler> _logger;

    public ManageMenuAuthorizationHandler(ApplicationDbContext context, ILogger<ManageMenuAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageMenuRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return;

        try
        {
            var query = _context.RestaurantEmployees
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.IsActive);

            if (requirement.MenuId.HasValue)
            {
                query = query.Where(e =>
                    e.Restaurant.Menu != null &&
                    e.Restaurant.Menu.Id == requirement.MenuId.Value);
            }
            else if (requirement.RestaurantId.HasValue)
            {
                query = query.Where(e => e.RestaurantId == requirement.RestaurantId.Value);
            }
            else
            {
                return;
            }

            var hasPermission = await query
                .SelectMany(e => e.Permissions)
                .AnyAsync(p => p.Permission == PermissionType.ManageMenu);

            if (hasPermission)
                context.Succeed(requirement);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization error in ManageMenuAuthorizationHandler");
        }
    }
}
