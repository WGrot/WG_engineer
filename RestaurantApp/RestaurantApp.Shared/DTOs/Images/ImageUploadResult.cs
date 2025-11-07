namespace RestaurantApp.Shared.DTOs.Images;

public class ImageUploadResult
{
    public string OriginalUrl { get; set; }
    public string ThumbnailUrl { get; set; }
    public string FileName { get; set; }
    public long? FileSize { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}