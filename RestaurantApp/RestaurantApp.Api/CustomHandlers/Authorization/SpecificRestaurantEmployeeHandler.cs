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
        // Pobierz HttpContext
        var httpContext = context.Resource as HttpContext;
        if (httpContext == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }
        
        var user = context.User;

        // Pobierz restaurantId z route
        var restaurantIdValue = httpContext.GetRouteValue("restaurantId")?.ToString();
        if (string.IsNullOrEmpty(restaurantIdValue) || !int.TryParse(restaurantIdValue, out var restaurantId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;


        // Sprawdź czy użytkownik jest pracownikiem tej restauracji
        var isEmployee = context.User.Claims.Any(c =>
            c.Type == "restaurant_employee" &&
            c.Value == restaurantId.ToString());

        if (!isEmployee)
        {
            context.Fail();
            return Task.CompletedTask;
        }


        // Sprawdź uprawnienia jeśli wymagane
        if (requirement.RequiredPermissions?.Any() == true)
        {
            var userPermissions = context.User.Claims
                .Where(c => c.Type == $"restaurant:{restaurantId}:permission")
                .Select(c => c.Value)
                .ToList();

            // Sprawdź czy ma wszystkie wymagane uprawnienia
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