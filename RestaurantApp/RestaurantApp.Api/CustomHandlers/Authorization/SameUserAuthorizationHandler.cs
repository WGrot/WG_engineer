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
    
        // Najpierw spróbuj pobrać z Route (dla ścieżek typu /users/{userId})
        var userIdFromRequest = httpContext.Request.RouteValues[requirement.ParameterName]?.ToString();

        // Jeśli nie ma w Route, spróbuj pobrać z Query (dla /employees?userId=...)
        if (string.IsNullOrEmpty(userIdFromRequest))
        {
            userIdFromRequest = httpContext.Request.Query[requirement.ParameterName].FirstOrDefault();
        }

        if (string.IsNullOrEmpty(userIdFromRequest))
        {
            return Task.CompletedTask;
        }
    
        var loggedInUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
        if (!string.IsNullOrEmpty(loggedInUserId) && loggedInUserId == userIdFromRequest)
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}