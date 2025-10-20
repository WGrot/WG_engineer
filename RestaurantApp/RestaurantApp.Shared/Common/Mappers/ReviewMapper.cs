using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Shared.Common.Mappers;

public static class ReviewMapper
{
    public static ReviewDto ToDto(this Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            RestaurantId = review.RestaurantId,
            RestaurantName = review.Restaurant?.Name ?? "Unknown",
            UserId = review.UserId,
            UserName = review.UserName,
            Rating = review.Rating,
            Content = review.Content,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            PhotosUrls = review.PhotosUrls,
            IsVerified = review.IsVerified,
            IsActive = review.IsActive,
            RestaurantResponse = review.RestaurantResponse != null ? ToDto(review.RestaurantResponse) : null
        };
    }
    
    
    public static List<ReviewDto> ToDtoList(this IEnumerable<Review> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
    }
    public static RestaurantReviewResponse ToDto(this RestaurantReviewResponse response)
    {
        return new RestaurantReviewResponse
        {
            Id = response.Id,
            Content = response.Content,
            CreatedAt = response.CreatedAt
        };
    }
    
    public static Review ToEntity(this CreateReviewDto dto, string userId, string userName, Restaurant restaurant)
    {
        return new Review
        {
            RestaurantId = dto.RestaurantId,
            Restaurant = restaurant,
            UserId = userId,
            UserName = userName,
            Rating = dto.Rating,
            Content = dto.Content,
            PhotosUrls = dto.PhotosUrls,
            CreatedAt = DateTime.UtcNow,
            IsVerified = false,
            IsActive = true
        };
    }
    
    public static void UpdateEntity(this Review review, UpdateReviewDto dto)
    {
        review.Rating = dto.Rating;
        review.Content = dto.Content;
        review.PhotosUrls = dto.PhotosUrls;
        review.UpdatedAt = DateTime.UtcNow;
    }
}