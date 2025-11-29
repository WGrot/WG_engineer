using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItemTags;

public class ManageTagsAuthorizationHandler : AuthorizationHandler<ManageTagsRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManageTagsAuthorizationHandler> _logger;

    public ManageTagsAuthorizationHandler(ApplicationDbContext context, ILogger<ManageTagsAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageTagsRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            var query = _context.RestaurantEmployees
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.IsActive);

            query = query.Where(e =>
                e.Restaurant.MenuItemTags.Any(t => t.Id == requirement.TagId));

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
            _logger.LogError(ex, "Authorization error in ManageTagsAuthorizationHandler");
            context.Fail();
        }
    }
}