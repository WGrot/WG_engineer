using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs;

public class ResponseRestaurantEmployeeDto
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string UserId { get; set; }
    public int RestaurantId { get; set; }
    public RestaurantDto Restaurant { get; set; }
    public RestaurantRole Role { get; set; }
    public List<RestaurantPermissionDto> Permissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}