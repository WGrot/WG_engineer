using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Permissions;

public class RestaurantPermissionDto
{
    public int Id { get; set; }
    public int RestaurantEmployeeId { get; set; }
    public PermissionType Permission { get; set; }
}