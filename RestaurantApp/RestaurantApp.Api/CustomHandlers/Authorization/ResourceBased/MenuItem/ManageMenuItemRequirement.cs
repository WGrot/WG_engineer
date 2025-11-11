using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItem;

public class ManageMenuItemRequirement: IAuthorizationRequirement
{
    public int MenuItemId { get; }

    public ManageMenuItemRequirement(int menuItemId)
    {
        MenuItemId = menuItemId;
    }
}