using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Models.DTOs;

public class CreateRestaurantPermissionDto
{
    public int RestaurantEmployeeId { get; set; }
    public PermissionType Permission { get; set; }
}