using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Permission;

public class ManagePermissionAuthorizationHandler: AuthorizationHandler<ManagePermissionRequirement>
{
    private readonly ApiDbContext _context;

    public ManagePermissionAuthorizationHandler(ApiDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManagePermissionRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {

            var permission = await _context.RestaurantPermissions
                .AsNoTracking()
                .Include(p => p.RestaurantEmployee)           
                .FirstOrDefaultAsync(v => v.Id == requirement.PermissionId);

            if (permission?.RestaurantEmployee.RestaurantId == null)
            {
                context.Fail();
                return;
            }

            var restaurantId = permission.RestaurantEmployee.RestaurantId;
            
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

            var hasManagePermission = await _context.RestaurantPermissions
                .AsNoTracking()
                .AnyAsync(ep =>
                    ep.RestaurantEmployeeId == employee.Id &&
                    ep.Permission == PermissionType.ManagePermissions);

            if (hasManagePermission)
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