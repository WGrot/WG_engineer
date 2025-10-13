using RestaurantApp.Api.Services.Interfaces;
using SkiaSharp;

namespace RestaurantApp.Api.Services;

public class ImageProcessor: IImageProcessor
{
    public SKBitmap ResizeImage(SKBitmap original, int maxWidth, int maxHeight)
    {
        var ratioX = (float)maxWidth / original.Width;
        var ratioY = (float)maxHeight / original.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(original.Width * ratio);
        var newHeight = (int)(original.Height * ratio);

        // Resize z wysoką jakością
        var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        
        if (resized == null)
        {
            throw new InvalidOperationException("Failed to resize image");
        }

        return resized;
    }

    public SKBitmap CreateSquareThumbnail(SKBitmap original, int size)
    {
        int sourceSize = Math.Min(original.Width, original.Height);
        int xOffset = (original.Width - sourceSize) / 2;
        int yOffset = (original.Height - sourceSize) / 2;

        // Najpierw cropuj do kwadratu
        var cropped = new SKBitmap(sourceSize, sourceSize);
        using (var canvas = new SKCanvas(cropped))
        {
            var sourceRect = SKRect.Create(xOffset, yOffset, sourceSize, sourceSize);
            var destRect = SKRect.Create(0, 0, sourceSize, sourceSize);
            canvas.DrawBitmap(original, sourceRect, destRect);
        }

        // Następnie zmniejsz do docelowego rozmiaru
        var thumbnail = cropped.Resize(new SKImageInfo(size, size), SKFilterQuality.High);
        cropped.Dispose();

        if (thumbnail == null)
        {
            throw new InvalidOperationException("Failed to create thumbnail");
        }

        return thumbnail;
    }

    public void SaveImageAsJpeg(SKBitmap bitmap, Stream stream, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        data.SaveTo(stream);
    }

    public SKBitmap Decode(Stream imageStream)
    {
        using var memory = new MemoryStream();
        imageStream.CopyTo(memory);
        memory.Position = 0;

        var bitmap = SKBitmap.Decode(memory);
        if (bitmap == null)
            throw new InvalidOperationException("Invalid or corrupted image stream.");

        return bitmap;
    }
}