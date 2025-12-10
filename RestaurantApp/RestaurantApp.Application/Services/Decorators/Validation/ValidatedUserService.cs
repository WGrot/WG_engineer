using FluentValidation;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Services.Decorators.Validation;

public class ValidatedUserService : IUserService
{
    private readonly IUserService _inner;
    private readonly IValidator<CreateUserDto> _createValidator;
    private readonly IValidator<UpdateUserDto> _updateValidator;
    private readonly IUserValidator _businessValidator;

    public ValidatedUserService(
        IUserService inner,
        IValidator<CreateUserDto> createValidator,
        IValidator<UpdateUserDto> updateValidator,
        IUserValidator businessValidator)
    {
        _inner = inner;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _businessValidator = businessValidator;
    }

    public async Task<Result<ResponseUserDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var businessResult = await _businessValidator.ValidateUserExistsAsync(id, ct);
        if (!businessResult.IsSuccess)
            return Result<ResponseUserDto>.From(businessResult);

        return await _inner.GetByIdAsync(id, ct);
    }

    public async Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(
        string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount,
        CancellationToken ct = default)
    {
        return await _inner.SearchAsync(firstName, lastName, phoneNumber, email, amount, ct);
    }

    public async Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto, CancellationToken ct = default)
    {
        var fluentResult = await _createValidator.ValidateAsync(userDto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult<CreateUserDto>();

        var businessResult = await _businessValidator.ValidateForCreateAsync(userDto, ct);
        if (!businessResult.IsSuccess)
            return Result<CreateUserDto>.From(businessResult);

        return await _inner.CreateAsync(userDto, ct);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        var fluentResult = await _updateValidator.ValidateAsync(dto, ct);
        if (!fluentResult.IsValid)
            return fluentResult.ToResult();

        var businessResult = await _businessValidator.ValidateForUpdateAsync(dto, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.UpdateUserAsync(dto, ct);
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        var businessResult = await _businessValidator.ValidateForDeleteAsync(userId, ct);
        if (!businessResult.IsSuccess)
            return businessResult;

        return await _inner.DeleteUserAsync(userId, ct);
    }
}