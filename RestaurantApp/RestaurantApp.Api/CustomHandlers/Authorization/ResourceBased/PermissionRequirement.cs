using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.NewDirectory1;

public class PermissionRequirement: IAuthorizationRequirement
{
    public PermissionTypeEnumDto PermissionName { get; }
    public PermissionRequirement(PermissionTypeEnumDto permissionName)
    {
        PermissionName = permissionName;
    }
}
