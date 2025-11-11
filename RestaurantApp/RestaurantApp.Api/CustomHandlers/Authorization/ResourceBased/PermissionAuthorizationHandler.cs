using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased;

public class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement, int>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement,
        int restaurantId)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Sprawdź, czy użytkownik jest pracownikiem restauracji
        var isEmployee = context.User.Claims.Any(c =>
            c.Type == "restaurant_employee" &&
            c.Value == restaurantId.ToString());

        if (!isEmployee)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Sprawdź uprawnienie
        var permissionClaimType = $"restaurant:{restaurantId}:permission";
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == permissionClaimType &&
            string.Equals(c.Value, requirement.PermissionName.ToString(), StringComparison.OrdinalIgnoreCase));

        if (hasPermission)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}