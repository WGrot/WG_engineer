using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Employees;

public class CreateInvitationDto
{
    public int RestaurantId { get; set; }
    public string UserId { get; set; }
    public RestaurantRoleEnumDto Role { get; set; }
}