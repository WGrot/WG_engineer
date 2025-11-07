using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs;

public class UpdateEmployeeDto
{
    public RestaurantRole Role { get; set; }
    public List<RestaurantPermissionDto> Permissions { get; set; } = new List<RestaurantPermissionDto>();
    public bool IsActive { get; set; }
}