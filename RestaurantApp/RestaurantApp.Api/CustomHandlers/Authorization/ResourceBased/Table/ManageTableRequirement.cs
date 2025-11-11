using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Table;

public class ManageTableRequirement : IAuthorizationRequirement
{
    public int? TableId { get; }
    public int? RestaurantId { get; }

    public ManageTableRequirement(int? tableId = null, int? restaurantId = null)
    {
        TableId = tableId;
        RestaurantId = restaurantId;
    }
}