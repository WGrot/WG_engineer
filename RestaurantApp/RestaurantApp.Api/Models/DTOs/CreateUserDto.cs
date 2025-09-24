using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Models.DTOs;

public class CreateUserDto
{
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    
    public int? RestaurantId { get; set; } 
    
    public string? Password { get; set; }
    public RestaurantRole? Role { get; set; }
}