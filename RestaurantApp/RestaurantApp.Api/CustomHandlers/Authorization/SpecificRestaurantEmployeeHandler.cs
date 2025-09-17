using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SpecificRestaurantEmployeeHandler: AuthorizationHandler<SpecificRestaurantEmployeeRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public SpecificRestaurantEmployeeHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SpecificRestaurantEmployeeRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = context.User;

        // Pobierz restaurantId z route
        var restaurantId = httpContext.GetRouteValue("restaurantId")?.ToString();
        
        if (string.IsNullOrEmpty(restaurantId))
        {
            // Spróbuj pobrać z query string jako fallback
            restaurantId = httpContext.Request.Query["restaurantId"].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(restaurantId))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Sprawdź czy użytkownik ma dostęp do TEJ KONKRETNEJ restauracji
        var restaurantClaim = user.Claims
            .FirstOrDefault(c => c.Type == "RestaurantAccess" && 
                                 c.Value.StartsWith($"{restaurantId}:"));

        if (restaurantClaim == null)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Parsuj rolę użytkownika w tej restauracji
        var parts = restaurantClaim.Value.Split(':');
        if (!Enum.TryParse<RestaurantRole>(parts[1], out var userRole))
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Sprawdź minimalną rolę jeśli wymagana
        if (requirement.MinimumRole.HasValue && userRole > requirement.MinimumRole.Value)
        {
            context.Fail();
            return Task.CompletedTask;
        }

        // Sprawdź konkretne uprawnienia jeśli wymagane
        if (requirement.RequiredPermissions?.Any() == true)
        {
            var userPermissions = user.Claims
                .Where(c => c.Type == "RestaurantPermission" && 
                           c.Value.StartsWith($"{restaurantId}:"))
                .Select(c => c.Value.Split(':')[1])
                .ToList();

            foreach (var requiredPermission in requirement.RequiredPermissions)
            {
                if (!userPermissions.Contains(requiredPermission.ToString()))
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