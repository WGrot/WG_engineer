using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Domain.Models;

public class RestaurantEmployee
{
    public int Id { get; set; }
    public string UserId { get; set; }
    
    public ApplicationUser User { get; set; }
    public int RestaurantId { get; set; }
    public Restaurant Restaurant { get; set; }
    public RestaurantRole Role { get; set; }
    public List<RestaurantPermission> Permissions { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}