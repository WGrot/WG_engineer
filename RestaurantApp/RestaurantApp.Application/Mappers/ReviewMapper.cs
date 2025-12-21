using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Review;


namespace RestaurantApp.Application.Mappers;

public static class ReviewMapper
{
    public static ReviewDto ToDto(this Review review)
    {
        return new ReviewDto
        {
            Id = review.Id,
            RestaurantId = review.RestaurantId,
            RestaurantName = review.Restaurant?.Name ?? string.Empty,
            RestaurantAddress = review.Restaurant?.Address ?? string.Empty,
            UserId = review.UserId,
            UserName = review.UserName,
            Rating = review.Rating,
            Content = review.Content,
            CreatedAt = review.CreatedAt,
            UpdatedAt = review.UpdatedAt,
            PhotosUrls = review.PhotosUrls,
            IsVerified = review.IsVerified,
            IsActive = review.IsActive,
        };
    }
    
    
    public static List<ReviewDto> ToDtoList(this IEnumerable<Review> entities)
    {
        return entities.Select(e => e.ToDto()).ToList();
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