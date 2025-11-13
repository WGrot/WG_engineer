using Microsoft.AspNetCore.Authorization;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Permission;

public class ManagePermissionRequirement: IAuthorizationRequirement
{
    public int PermissionId { get; }

    public ManagePermissionRequirement(int permissionId)
    {
        PermissionId = permissionId;
    }
}