using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Table;

public class ManageTableAuthorizationHandler: AuthorizationHandler<ManageTableRequirement>
{
    private readonly ApiDbContext _context;

    public ManageTableAuthorizationHandler(ApiDbContext context)
    {
        _context = context;
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
            int? restaurantId = 0;
            if (requirement.RestaurantId == null)
            {
                restaurantId = await _context.Tables
                    .AsNoTracking()
                    .Where(t => t.Id == requirement.TableId)
                    .Select(t => t.RestaurantId)
                    .FirstOrDefaultAsync();
            }
            else
            {
                restaurantId = requirement.RestaurantId.Value;
            }

            

            if (restaurantId == 0)
            {
                context.Fail();
                return;
            }

            var hasPermission = await _context.RestaurantEmployees
                .AsNoTracking()
                .Where(er =>
                    er.UserId == userId &&
                    er.RestaurantId == restaurantId &&
                    er.IsActive)
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
        catch (Exception)
        {
            context.Fail();
        }
    }
}