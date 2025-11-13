using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SameUserAuthorizationHandler: AuthorizationHandler<SameUserRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        SameUserRequirement requirement)
    {

        var currentUserId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (currentUserId != null && currentUserId == requirement.UserId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}