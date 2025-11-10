using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

public class ManageMenuRequirement : IAuthorizationRequirement
{
    public int? MenuId { get; }

    public ManageMenuRequirement(int? menuId = null)
    {
        MenuId = menuId;
    }
}