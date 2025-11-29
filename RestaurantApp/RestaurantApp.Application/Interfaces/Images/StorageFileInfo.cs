namespace RestaurantApp.Application.Interfaces.Images;

public class StorageFileInfo
{
    public string Key { get; set; } = string.Empty;
    public long? Size { get; set; }
    public DateTime? LastModified { get; set; }
    public string? ContentType { get; set; }
    public string? ETag { get; set; }
}