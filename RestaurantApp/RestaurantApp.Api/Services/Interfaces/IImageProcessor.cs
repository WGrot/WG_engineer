using SkiaSharp;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IImageProcessor
{
    public SKBitmap ResizeImage(SKBitmap original, int maxWidth, int maxHeight);
    public SKBitmap CreateSquareThumbnail(SKBitmap original, int size);
    void SaveImageAsJpeg(SKBitmap bitmap, Stream stream, int quality);
    public SKBitmap Decode(Stream imageStream);
}