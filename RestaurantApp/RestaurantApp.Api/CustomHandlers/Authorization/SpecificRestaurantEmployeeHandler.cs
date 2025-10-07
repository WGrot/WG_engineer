using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SpecificRestaurantEmployeeHandler : AuthorizationHandler<SpecificRestaurantEmployeeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SpecificRestaurantEmployeeRequirement requirement)
    {
        var httpContext = context.Resource as HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }


        var restaurantIdHeader = httpContext.Request.Headers["X-Restaurant-Id"].FirstOrDefault();
        if (string.IsNullOrEmpty(restaurantIdHeader) || !int.TryParse(restaurantIdHeader, out var restaurantId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        var isEmployee = context.User.Claims.Any(c =>
            c.Type == "restaurant_employee" &&
            c.Value == restaurantId.ToString());

        if (!isEmployee)
        {
            context.Fail();
            return Task.CompletedTask;
        }


        if (requirement.RequiredPermissions?.Any() == true)
        {
            var userPermissions = context.User.Claims
                .Where(c => c.Type == $"restaurant:{restaurantId}:permission")
                .Select(c => c.Value)
                .ToList();

            foreach (var permission in requirement.RequiredPermissions)
            {
                if (!userPermissions.Contains(permission.ToString()))
                {
                    context.Fail();
                    return Task.CompletedTask;
                }
            }
        }

        context.Succeed(requirement);
        return Task.CompletedTask;
    }
}