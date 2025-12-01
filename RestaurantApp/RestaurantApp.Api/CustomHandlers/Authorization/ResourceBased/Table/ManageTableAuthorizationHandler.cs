using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Table;

public class ManageTableAuthorizationHandler : AuthorizationHandler<ManageTableRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManageTableAuthorizationHandler> _logger;

    public ManageTableAuthorizationHandler(
        ApplicationDbContext context,
        ILogger<ManageTableAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageTableRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            var query = _context.RestaurantEmployees
                .AsNoTracking()
                .Where(er => er.UserId == userId && er.IsActive);
            
            if (requirement.RestaurantId.HasValue)
            {
                query = query.Where(er => er.RestaurantId == requirement.RestaurantId.Value);
            }
            else
            {
                query = query.Where(er => er.Restaurant.Tables
                    .Any(t => t.Id == requirement.TableId));
            }
            
            var hasPermission = await query
                .SelectMany(er => er.Permissions)
                .AnyAsync(p => p.Permission == PermissionType.ManageTables);

            if (hasPermission)
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
            _logger.LogError(ex, 
                "Authorization failed for user {UserId}, table {TableId}, restaurant {RestaurantId}",
                userId, requirement.TableId, requirement.RestaurantId);
            context.Fail();
        }
    }
}