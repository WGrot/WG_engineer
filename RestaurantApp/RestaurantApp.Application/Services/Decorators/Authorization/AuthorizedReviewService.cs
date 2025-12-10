using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Services.Decorators.Authorization;

public class AuthorizedReviewService : IReviewService
{
    private readonly IReviewService _inner;
    private readonly ICurrentUserService _currentUser;
    private readonly IAuthorizationChecker _authorizationChecker;

    public AuthorizedReviewService(
        IReviewService inner,
        ICurrentUserService currentUser,
        IAuthorizationChecker authorizationChecker)
    {
        _inner = inner;
        _currentUser = currentUser;
        _authorizationChecker = authorizationChecker;
    }

    public async Task<Result<ReviewDto>> GetByIdAsync(int id)
    {
        return await _inner.GetByIdAsync(id);
    }

    public async Task<Result<List<ReviewDto>>> GetAllAsync()
    {
        return await _inner.GetAllAsync();
    }

    public async Task<Result<List<ReviewDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        return await _inner.GetByRestaurantIdAsync(restaurantId);
    }

    public async Task<Result<List<ReviewDto>>> GetByUserIdAsync(string userId)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<List<ReviewDto>>.Forbidden("You don't have permission to view reviews of other users.");
        }
        
        return await _inner.GetByUserIdAsync(userId);
    }

    public async Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<ReviewDto>.Forbidden("You don't have permission to view reviews of other users.");
        }
        return await _inner.CreateAsync(userId, dto);
    }

    public async Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<ReviewDto>.Forbidden("You don't have permission to view reviews of other users.");
        }
        return await _inner.UpdateAsync(userId, id, dto);
    }

    public async Task<Result> DeleteAsync(string userId, int id)
    {
        if (_currentUser.UserId != userId)
        {
            return Result<ReviewDto>.Forbidden("You don't have permission to view reviews of other users.");
        }
        return await _inner.DeleteAsync(userId, id);
    }

    public Task<Result> ToggleActiveStatusAsync(int id)
    {
        return _inner.ToggleActiveStatusAsync(id);
    }

    public Task<Result> VerifyReviewAsync(int id)
    {
        return _inner.VerifyReviewAsync(id);
    }

    public Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId, int page, int pageSize,
        string? sortBy)
    {
        return _inner.GetByRestaurantIdPaginatedAsync(restaurantId, page, pageSize, sortBy);
    }
    
}