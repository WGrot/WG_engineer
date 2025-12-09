namespace RestaurantApp.Shared.DTOs.Images;

public class ImageLinkDto
{
    public int Id { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public int SortOrder { get; set; } = 0;
    public string? OriginalFileName { get; set; }
    
}