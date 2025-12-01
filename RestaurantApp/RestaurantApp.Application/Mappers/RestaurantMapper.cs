using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Restaurant;

namespace RestaurantApp.Application.Mappers;

public static class RestaurantMapper
{

    public static RestaurantDto ToDto(this Restaurant entity)
    {
        return new RestaurantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Address = entity.Address,
            Location = entity.Location?.ToDto(),
            StructuredAddress = entity.StructuredAddress?.ToDto(),
            Description = entity.Description,
            OpeningHours = entity.OpeningHours?.Select(oh => oh.ToDto()).ToList(),
            ProfileUrl = entity.profileUrl,
            ProfileThumbnailUrl = entity.profileThumbnailUrl,
            PhotosUrls = entity.photosUrls,
            PhotosThumbnailsUrls = entity.photosThumbnailsUrls,
            AverageRating = entity.AverageRating,
            TotalReviews = entity.TotalReviews,
            TotalRatings1Star = entity.TotalRatings1Star,
            TotalRatings2Star = entity.TotalRatings2Star,
            TotalRatings3Star = entity.TotalRatings3Star,
            TotalRatings4Star = entity.TotalRatings4Star,
            TotalRatings5Star = entity.TotalRatings5Star
        };
    }


    public static Restaurant ToEntity(this RestaurantDto dto)
    {
        return new Restaurant
        {
            Id = dto.Id,
            Name = dto.Name,
            Address = dto.Address,
            StructuredAddress = dto.StructuredAddress?.ToEntity(),
            Location = dto.Location?.ToEntity(),
            Description = dto.Description,
            OpeningHours = dto.OpeningHours?.Select(oh => oh.ToEntity()).ToList(),
            profileUrl = dto.ProfileUrl,
            profileThumbnailUrl = dto.ProfileThumbnailUrl,
            photosUrls = dto.PhotosUrls,
            photosThumbnailsUrls = dto.PhotosThumbnailsUrls,
            AverageRating = dto.AverageRating,
            TotalReviews = dto.TotalReviews,
            TotalRatings1Star = dto.TotalRatings1Star,
            TotalRatings2Star = dto.TotalRatings2Star,
            TotalRatings3Star = dto.TotalRatings3Star,
            TotalRatings4Star = dto.TotalRatings4Star,
            TotalRatings5Star = dto.TotalRatings5Star

        };
    }
    
    public static void UpdateFromDto(this Restaurant entity, RestaurantDto dto)
    {
        entity.Name = dto.Name;
        entity.Address = dto.Address;
        entity.Description = dto.Description;
        

        if (dto.OpeningHours != null)
        {
            entity.OpeningHours = dto.OpeningHours.Select(oh => oh.ToEntity()).ToList();
        }
        
        entity.profileUrl = dto.ProfileUrl;
        entity.profileThumbnailUrl = dto.ProfileThumbnailUrl;
        entity.photosUrls = dto.PhotosUrls;
        entity.photosThumbnailsUrls = dto.PhotosThumbnailsUrls;
        

        entity.AverageRating = dto.AverageRating;
        entity.TotalReviews = dto.TotalReviews;
        entity.TotalRatings1Star = dto.TotalRatings1Star;
        entity.TotalRatings2Star = dto.TotalRatings2Star;
        entity.TotalRatings3Star = dto.TotalRatings3Star;
        entity.TotalRatings4Star = dto.TotalRatings4Star;
        entity.TotalRatings5Star = dto.TotalRatings5Star;

    }


    public static List<RestaurantDto> ToDtoList(this IEnumerable<Restaurant> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }


    public static List<Restaurant> ToEntityList(this IEnumerable<RestaurantDto> dtos)
    {
        return dtos.Select(d => d.ToEntity()).ToList();
    }
}