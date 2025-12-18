using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.DTOs.Users;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public NotificationTypeEnumDto Type { get; set; }
    public NotificationCategoryEnumDto Category { get; set; }
    public string? ActionUrl { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}