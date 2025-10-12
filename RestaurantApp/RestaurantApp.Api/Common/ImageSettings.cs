﻿using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Common;

public class ImageSettings
{
    public ImageSizeConfig UserProfile { get; set; }
    public ImageSizeConfig RestaurantProfile { get; set; }
    public ImageSizeConfig RestaurantBackground { get; set; }
    public ImageSizeConfig MenuItem { get; set; }
    public ImageSizeConfig RestaurantPhotos { get; set; }

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