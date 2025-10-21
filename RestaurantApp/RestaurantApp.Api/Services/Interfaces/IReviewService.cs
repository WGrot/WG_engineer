using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IReviewService
{
    Task<Result<ReviewDto>> GetByIdAsync(int id);
    Task<Result<List<ReviewDto>>> GetAllAsync();
    Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId);
    Task<Result<ReviewDto>> CreateAsync(CreateReviewDto createReviewDto);
    Task<Result<ReviewDto>> UpdateAsync(int id, UpdateReviewDto updateReviewDto);
    Task<Result> DeleteAsync(int id);
    Task<Result> ToggleActiveStatusAsync(int id);
    Task<Result> VerifyReviewAsync(int id);

    Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string sortBy);
}