using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Permissions;

public class CreateRestaurantPermissionDto
{
    public int RestaurantEmployeeId { get; set; }
    public PermissionType Permission { get; set; }
}