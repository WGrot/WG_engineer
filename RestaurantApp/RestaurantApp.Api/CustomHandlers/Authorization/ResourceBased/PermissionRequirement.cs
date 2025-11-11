using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

public class PermissionRequirement: IAuthorizationRequirement
{
    public PermissionType PermissionName { get; }
    public PermissionRequirement(PermissionType permissionName)
    {
        PermissionName = permissionName;
    }
}
