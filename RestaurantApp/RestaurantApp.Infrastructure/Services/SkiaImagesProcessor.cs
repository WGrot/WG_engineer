using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
using SkiaSharp;

namespace RestaurantApp.Infrastructure.Services;

public class SkiaImageProcessor : IImageProcessor
{
    public async Task<ImageProcessingResult> ProcessImageAsync(
        Stream inputStream,
        int maxWidth,
        int maxHeight,
        int quality = 85)
    {
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var originalBitmap = SKBitmap.Decode(memoryStream);
        if (originalBitmap == null)
        {
            throw new InvalidOperationException("Invalid image format or corrupted file");
        }

        var needsResize = originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight;
        SKBitmap processedBitmap = originalBitmap;

        try
        {
            if (needsResize)
            {
                processedBitmap = ResizeImage(originalBitmap, maxWidth, maxHeight);
            }

            var outputStream = new MemoryStream();
            SaveAsJpeg(processedBitmap, outputStream, quality);
            outputStream.Position = 0;

            return new ImageProcessingResult(
                outputStream,
                processedBitmap.Width,
                processedBitmap.Height,
                outputStream.Length);
        }
        finally
        {
            if (needsResize && processedBitmap != originalBitmap)
            {
                processedBitmap.Dispose();
            }
        }
    }

    public async Task<Stream> CreateThumbnailAsync(
        Stream inputStream, 
        int size, 
        int quality = 75)
    {
        using var memoryStream = new MemoryStream();
        await inputStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        using var originalBitmap = SKBitmap.Decode(memoryStream);
        if (originalBitmap == null)
        {
            throw new InvalidOperationException("Invalid image format or corrupted file");
        }

        using var thumbnail = CreateSquareThumbnail(originalBitmap, size);
        
        var outputStream = new MemoryStream();
        SaveAsJpeg(thumbnail, outputStream, quality);
        outputStream.Position = 0;

        return outputStream;
    }

public async Task<ImageInfo?> GetImageInfoAsync(Stream imageStream)
{
    try
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream);
        
        
        memoryStream.Position = 0;

        using var codec = SKCodec.Create(memoryStream);
        
        
        if (codec == null)
        {
            memoryStream.Position = 0;
            using var bitmap = SKBitmap.Decode(memoryStream);
            
            if (bitmap != null)
            {
                return new ImageInfo(bitmap.Width, bitmap.Height, true);
            }
            
            return null;
        }

        return new ImageInfo(
            codec.Info.Width,
            codec.Info.Height,
            true);
    }
    catch (Exception ex)
    {
        return null;
    }
}

    #region Private Methods

    private SKBitmap ResizeImage(SKBitmap original, int maxWidth, int maxHeight)
    {
        var ratioX = (float)maxWidth / original.Width;
        var ratioY = (float)maxHeight / original.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(original.Width * ratio);
        var newHeight = (int)(original.Height * ratio);

        var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        
        return resized ?? throw new InvalidOperationException("Failed to resize image");
    }

    private SKBitmap CreateSquareThumbnail(SKBitmap original, int size)
    {
        int sourceSize = Math.Min(original.Width, original.Height);
        int xOffset = (original.Width - sourceSize) / 2;
        int yOffset = (original.Height - sourceSize) / 2;

        var cropped = new SKBitmap(sourceSize, sourceSize);
        using (var canvas = new SKCanvas(cropped))
        {
            var sourceRect = SKRect.Create(xOffset, yOffset, sourceSize, sourceSize);
            var destRect = SKRect.Create(0, 0, sourceSize, sourceSize);
            canvas.DrawBitmap(original, sourceRect, destRect);
        }

        var thumbnail = cropped.Resize(new SKImageInfo(size, size), SKFilterQuality.High);
        cropped.Dispose();

        return thumbnail ?? throw new InvalidOperationException("Failed to create thumbnail");
    }

    private void SaveAsJpeg(SKBitmap bitmap, Stream stream, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        data.SaveTo(stream);
    }

    #endregion
}
