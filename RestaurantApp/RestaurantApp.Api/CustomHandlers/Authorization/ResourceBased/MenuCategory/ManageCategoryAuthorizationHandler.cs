using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuCategory;

public class ManageCategoryAuthorizationHandler : AuthorizationHandler<ManageCategoryRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManageCategoryAuthorizationHandler> _logger;

    public ManageCategoryAuthorizationHandler(ApplicationDbContext context, ILogger<ManageCategoryAuthorizationHandler> logger)
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
            var query = _context.RestaurantEmployees
                .AsNoTracking()
                .Where(e => e.UserId == userId && e.IsActive);
            
            query = query.Where(e =>
                e.Restaurant.Menu != null &&
                e.Restaurant.Menu.Categories.Any(c => c.Id == requirement.CategoryId));
            
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