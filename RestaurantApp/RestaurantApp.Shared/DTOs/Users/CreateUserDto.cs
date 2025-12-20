using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Users;

public class CreateUserDto
{
    public required string Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    
    public int? RestaurantId { get; set; } 
    
    public string? Password { get; set; }
    public RestaurantRoleEnumDto? Role { get; set; }
}