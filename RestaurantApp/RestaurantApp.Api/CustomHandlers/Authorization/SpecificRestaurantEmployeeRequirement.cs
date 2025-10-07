using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SpecificRestaurantEmployeeRequirement : IAuthorizationRequirement
{
    public PermissionType[] RequiredPermissions { get; set; }
    
    public SpecificRestaurantEmployeeRequirement(params PermissionType[] permissions)
    {
        RequiredPermissions = permissions;
    }
    public SpecificRestaurantEmployeeRequirement()
    {

    }
}