using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Models.DTOs;

public class UpdateEmployeeDto
{
    public RestaurantRole Role { get; set; }
    public List<RestaurantPermission> Permissions { get; set; } = new List<RestaurantPermission>();
    public bool IsActive { get; set; }
}