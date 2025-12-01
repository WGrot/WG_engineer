using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;
namespace RestaurantApp.Application.Interfaces.Services;

public interface IReviewService
{
    Task<Result<ReviewDto>> GetByIdAsync(int id, CancellationToken ct = default);
    Task<Result<List<ReviewDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    
    Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto, CancellationToken ct = default);
    Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(string userId, int id, CancellationToken ct = default);
    
    Task<Result> ToggleActiveStatusAsync(int id, CancellationToken ct = default);
    Task<Result> VerifyReviewAsync(int id, CancellationToken ct = default);

    Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(
        int restaurantId,
        int page,
        int pageSize,
        string? sortBy,
        CancellationToken ct = default);
}