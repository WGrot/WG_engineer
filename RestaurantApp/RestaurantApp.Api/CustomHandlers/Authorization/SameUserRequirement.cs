using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SameUserRequirement: IAuthorizationRequirement
{

    public string UserId { get; }
    
    public SameUserRequirement(string userId)
    {
        UserId = userId;
    }
}