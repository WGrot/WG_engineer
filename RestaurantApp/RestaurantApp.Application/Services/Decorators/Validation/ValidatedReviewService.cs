using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedReviewService : IReviewService
{
    private readonly IReviewService _inner;
    private readonly IValidator<CreateReviewDto> _createValidator;
    private readonly IValidator<UpdateReviewDto> _updateValidator;
    private readonly IReviewValidator _businessValidator;

    public ValidatedReviewService(
        IReviewService inner,
        IValidator<CreateReviewDto> createValidator,
        IValidator<UpdateReviewDto> updateValidator,
        IReviewValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
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
        return await _inner.GetByUserIdAsync(userId);
    }

    public async Task<Result<ReviewDto>> CreateAsync(string userId, CreateReviewDto dto)
    {
        var fluentResult = await _createValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<ReviewDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(userId, dto);
        if (!businessResult.IsSuccess)
            return Result<ReviewDto>.From(businessResult);

        return await _inner.CreateAsync(userId, dto);
    }

    public async Task<Result<ReviewDto>> UpdateAsync(string userId, int id, UpdateReviewDto dto)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<ReviewDto>();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(userId, id, dto);
        if (!businessResult.IsSuccess)
            return Result<ReviewDto>.From(businessResult);

        return await _inner.UpdateAsync(userId, id, dto);
    }

    public async Task<Result> DeleteAsync(string userId, int id)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(userId, id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteAsync(userId, id);
    }

    public async Task<Result> ToggleActiveStatusAsync(int id)
    {
        var businessResult = await _businessValidator.ValidateReviewExistsAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.ToggleActiveStatusAsync(id);
    }

    public async Task<Result> VerifyReviewAsync(int id)
    {
        var businessResult = await _businessValidator.ValidateReviewExistsAsync(id);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.VerifyReviewAsync(id);
    }

    public async Task<Result<PaginatedReviewsDto>> GetByRestaurantIdPaginatedAsync(int restaurantId,
        int page,
        int pageSize,
        string? sortBy)
    {
        return await _inner.GetByRestaurantIdPaginatedAsync(restaurantId, page, pageSize, sortBy);
    }
}