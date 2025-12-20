using RestaurantApp.Domain.Enums;

namespace RestaurantApp.Application.Interfaces.Images;

public class ImageSettings
{
    public required ImageSizeConfig UserProfile { get; set; }
    public required ImageSizeConfig RestaurantProfile { get; set; }
    public required ImageSizeConfig RestaurantBackground { get; set; }
    public required ImageSizeConfig MenuItem { get; set; }
    public required ImageSizeConfig RestaurantPhotos { get; set; }

    public ImageSizeConfig GetConfig(ImageType imageType)
    {
        var config = imageType switch
        {
            ImageType.UserProfile => UserProfile,
            ImageType.RestaurantProfile => RestaurantProfile,
            ImageType.RestaurantBackground => RestaurantBackground,
            ImageType.MenuItem => MenuItem,
            ImageType.RestaurantPhotos => RestaurantPhotos,
            _ => throw new ArgumentException($"Unknown image type: {imageType}")
        };

        config.Validate();
        return config;
    }
}