using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedReviewService : IReviewService
{
    private readonly IReviewService _inner;
    private readonly ICurrentUserService _currentUser;


    public AuthorizedReviewService(
        IReviewService inner,
        ICurrentUserService currentUser)
    {
        _inner = inner;
        _currentUser = currentUser;

    }

    public async Task<Result<ReviewDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<List<ReviewDto>>> GetAllAsync(CancellationToken ct)
    {
        return await _inner.GetAllAsync(ct);
    }

    public async Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId, ct);
    }

    public async Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<List<ReviewDto>>.Forbidden("You don't have permission to view reviews of other users.");
        }
        
        return await _inner.GetByUserIdAsync(userId, ct);
    }

    public async Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto, CancellationToken ct)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<ReviewDto>.Forbidden("You don't have create reviews of other users.");
        }
        return await _inner.CreateAsync(userId, dto, ct);
    }

    public async Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto, CancellationToken ct)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<ReviewDto>.Forbidden("You don't have permission to edit reviews of other users.");
        }
        return await _inner.UpdateAsync(userId, id, dto, ct);
    }

    public async Task<Result> DeleteAsync(string userId, int id, CancellationToken ct)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<ReviewDto>.Forbidden("You don't have permission to delete reviews of other users.");
        }
        return await _inner.DeleteAsync(userId, id, ct);
    }

    public Task<Result> ToggleActiveStatusAsync(int id, CancellationToken ct)
    {
        return _inner.ToggleActiveStatusAsync(id, ct);
    }

    public Task<Result> VerifyReviewAsync(int id, CancellationToken ct)
    {
        return _inner.VerifyReviewAsync(id, ct);
    }

    public Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId, int page, int pageSize,
        string? sortBy, CancellationToken ct)
    {
        return _inner.GetByRestaurantIdPaginatedAsync(restaurantId, page, pageSize, sortBy, ct);
    }
    
}