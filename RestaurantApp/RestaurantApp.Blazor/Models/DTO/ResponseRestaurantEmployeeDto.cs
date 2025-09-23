using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Models.Response;

public class ResponseRestaurantEmployeeDto
{
    public string Id { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    
    public string? PhoneNumber { get; set; }
    
    public string UserId { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; }
    public RestaurantRole Role { get; set; }
    public List<RestaurantPermission> Permissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}