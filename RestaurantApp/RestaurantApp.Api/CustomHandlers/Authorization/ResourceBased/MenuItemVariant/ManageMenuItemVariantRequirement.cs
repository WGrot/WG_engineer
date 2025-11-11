using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.MenuItemVariant;

public class ManageMenuItemVariantRequirement: IAuthorizationRequirement
{
    public int MenuItemVariantId { get; }

    public ManageMenuItemVariantRequirement(int menuItemVariantId)
    {
        MenuItemVariantId = menuItemVariantId;
    }
}