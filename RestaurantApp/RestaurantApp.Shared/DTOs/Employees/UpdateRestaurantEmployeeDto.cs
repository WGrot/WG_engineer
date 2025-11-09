using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Employees;

public class UpdateEmployeeDto
{
    public int Id { get; set; }
    public RestaurantRole Role { get; set; }
    public bool IsActive { get; set; }
}