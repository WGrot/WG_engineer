using RestaurantApp.Application.Interfaces.Images;

namespace RestaurantApp.Application.Common.Interfaces;
public interface IImageProcessor
{
    Task<ImageProcessingResult> ProcessImageAsync(
        Stream inputStream,
        int maxWidth,
        int maxHeight,
        int quality = 85);
    Task<Stream> CreateThumbnailAsync(
        Stream inputStream, 
        int size, 
        int quality = 75);
    Task<ImageInfo?> GetImageInfoAsync(Stream imageStream);
}