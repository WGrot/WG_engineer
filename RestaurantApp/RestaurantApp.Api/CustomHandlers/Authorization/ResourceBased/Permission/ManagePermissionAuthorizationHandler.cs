using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Permission;

public class ManagePermissionAuthorizationHandler : AuthorizationHandler<ManagePermissionRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManagePermissionAuthorizationHandler> _logger;

    public ManagePermissionAuthorizationHandler(
        ApplicationDbContext context,
        ILogger<ManagePermissionAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
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
            // ✅ JEDNO zapytanie zamiast trzech!
            var hasPermission = await _context.RestaurantPermissions
                .AsNoTracking()
                .Where(p => p.Id == requirement.PermissionId)
                .Where(p => p.RestaurantEmployee.RestaurantId != null) // Walidacja
                .Join(
                    _context.RestaurantEmployees.AsNoTracking()
                        .Where(er => er.UserId == userId && er.IsActive),
                    targetPermission => targetPermission.RestaurantEmployee.RestaurantId,
                    currentEmployee => currentEmployee.RestaurantId,
                    (targetPermission, currentEmployee) => currentEmployee
                )
                .SelectMany(emp => emp.Permissions)
                .AnyAsync(p => p.Permission == PermissionType.ManagePermissions);

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
                "Authorization failed for user {UserId}, permission {PermissionId}",
                userId, requirement.PermissionId);
            context.Fail();
        }
    }
}