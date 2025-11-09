using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SameUserRequirement: IAuthorizationRequirement
{
    public string UserIdRouteParameter { get; }
    
    public SameUserRequirement(string userIdRouteParameter = "userId")
    {
        UserIdRouteParameter = userIdRouteParameter;
    }
}