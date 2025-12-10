using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;
namespace RestaurantApp.Application.Interfaces.Services;

public interface IReviewService
{
    Task<Result<ReviewDto>> GetByIdAsync(int id);
    Task<Result<List<ReviewDto>>> GetAllAsync();
    Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId);
    
    Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto);
    Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto);
    Task<Result> DeleteAsync(string userId, int id);
    
    Task<Result> ToggleActiveStatusAsync(int id);
    Task<Result> VerifyReviewAsync(int id);

    Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy);
}