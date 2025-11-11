using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Menu;

public class ManageMenuRequirement : IAuthorizationRequirement
{
    public int? MenuId { get; }
    public int? RestaurantId { get; }

    public ManageMenuRequirement(int? menuId = null, int? restaurantId = null)
    {
        MenuId = menuId;
        RestaurantId = restaurantId;
    }
}