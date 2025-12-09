using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Domain.Models;

public class ImageLink
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public ImageType Type { get; set; }
    public int SortOrder { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public long? FileSize { get; set; }
    public string? OriginalFileName { get; set; }
    
    public int? RestaurantId { get; set; }
    public int? MenuItemId { get; set; }
    
    public Restaurant? Restaurant { get; set; }
    public MenuItem? MenuItem { get; set; }
}