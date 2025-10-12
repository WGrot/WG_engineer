namespace RestaurantApp.Api.Common;

public class ImageSizeConfig
{
    public int MaxWidth { get; set; }
    public int MaxHeight { get; set; }
    public int ThumbnailSize { get; set; }
    public int Quality { get; set; } = 85;
    public string Format { get; set; } = "jpeg";

    public void Validate()
    {
        if (MaxWidth <= 0 || MaxHeight <= 0 || ThumbnailSize <= 0)
            throw new InvalidOperationException("Image dimensions must be positive");
        
        if (Quality < 1 || Quality > 100)
            throw new InvalidOperationException("Quality must be between 1 and 100");
    }
}