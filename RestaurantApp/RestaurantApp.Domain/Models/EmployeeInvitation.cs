using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Domain.Models;

public class EmployeeInvitation
{
    public int Id { get; set; }
    
    public int RestaurantId { get; set; }
    public string UserId { get; set; }
    
    public RestaurantRole Role { get; set; }
    
    public string Token { get; set; } = string.Empty;
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    
    public int NotificationId { get; set; }
    
    public UserNotification Notification { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    
    public Restaurant Restaurant { get; set; } = null!;
    public ApplicationUser User { get; set; } = null!;
}