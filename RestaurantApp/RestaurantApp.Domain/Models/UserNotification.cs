using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Domain.Models;

public class UserNotification
{
    public int Id { get; set; }
    public string UserId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    
    public bool IsRead { get; set; } = false;
    
    public NotificationCategory Category { get; set; }
    public string? ReferenceId { get; set; } 
    public string? ActionUrl { get; set; }  
    

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    
    public ApplicationUser User { get; set; } = null!;
}