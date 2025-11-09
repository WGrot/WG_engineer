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
    
        var userIdFromRoute = httpContext.Request.RouteValues[requirement.UserIdRouteParameter]?.ToString();

    
        if (string.IsNullOrEmpty(userIdFromRoute))
        {
            return Task.CompletedTask;
        }
    
        // Spróbuj różnych typów claims
        var loggedInUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
        if (!string.IsNullOrEmpty(loggedInUserId) && loggedInUserId == userIdFromRoute)
        {
            context.Succeed(requirement);
        }
    
        return Task.CompletedTask;
    }
}