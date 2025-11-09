using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SameUserAuthorizationHandler: AuthorizationHandler<SameUserRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public SameUserAuthorizationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        SameUserRequirement requirement)
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return Task.CompletedTask;
        }
    
        // DEBUG: Wypisz wszystkie claims
        var claims = context.User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        foreach (var claim in claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
            // Lub użyj loggera:
            // _logger.LogInformation($"Claim Type: {claim.Type}, Value: {claim.Value}");
        }
    
        var userIdFromRoute = httpContext.Request.RouteValues[requirement.UserIdRouteParameter]?.ToString();
        Console.WriteLine($"User ID from route: {userIdFromRoute}");
    
        if (string.IsNullOrEmpty(userIdFromRoute))
        {
            return Task.CompletedTask;
        }
    
        // Spróbuj różnych typów claims
        var loggedInUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.User.FindFirst("sub")?.Value
                             ?? context.User.FindFirst("user_id")?.Value
                             ?? context.User.FindFirst("id")?.Value
                             ?? context.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
    
        Console.WriteLine($"Logged in user ID: {loggedInUserId}");
    
        if (!string.IsNullOrEmpty(loggedInUserId) && loggedInUserId == userIdFromRoute)
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}