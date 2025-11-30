using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SpecificRestaurantEmployeeRequirement : IAuthorizationRequirement
{
    public PermissionTypeEnumDto[] RequiredPermissions { get; set; }
    
    public SpecificRestaurantEmployeeRequirement(params PermissionTypeEnumDto[] permissions)
    {
        RequiredPermissions = permissions;
    }
    public SpecificRestaurantEmployeeRequirement()
    {

    }
}