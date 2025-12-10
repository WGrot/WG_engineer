using Microsoft.AspNetCore.Identity;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Services.Validators;

public class UserValidator : IUserValidator
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserValidator(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public Task<Result> ValidateUserIdNotEmptyAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(Result.ValidationError("User ID cannot be null or empty."));

        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ValidateUserExistsAsync(string userId, CancellationToken ct = default)
    {
        var idResult = await ValidateUserIdNotEmptyAsync(userId, ct);
        if (!idResult.IsSuccess)
            return idResult;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.NotFound($"User with ID '{userId}' was not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateEmailUniqueAsync(string email, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return Result.Conflict("User with this email already exists.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateUserDto dto, CancellationToken ct = default)
    {
        return await ValidateEmailUniqueAsync(dto.Email, ct);
    }

    public async Task<Result> ValidateForUpdateAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        return await ValidateUserExistsAsync(dto.Id, ct);
    }

    public async Task<Result> ValidateForDeleteAsync(string userId, CancellationToken ct = default)
    {
        return await ValidateUserExistsAsync(userId, ct);
    }
}