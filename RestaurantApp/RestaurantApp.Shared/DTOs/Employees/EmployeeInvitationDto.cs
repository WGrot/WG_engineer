using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Employees;

public class EmployeeInvitationDto
{
    public int Id { get; set; }
    public int RestaurantId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public RestaurantRoleEnumDto Role { get; set; }
    public InvitationStatusEnumDto Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}