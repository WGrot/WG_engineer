using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Permissions;

public class UpdateEmployeePermisionsDto
{
    public int EmployeeId { get; set; }
    
    public int RestaurantId { get; set; }
    public List<PermissionType> Permissions { get; set; }
}