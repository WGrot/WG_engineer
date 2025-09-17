using Microsoft.AspNetCore.Authorization;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization;

public class SpecificRestaurantEmployeeRequirement : IAuthorizationRequirement
{
    public PermissionType[] RequiredPermissions { get; set; }
    public RestaurantRole? MinimumRole { get; set; }
}