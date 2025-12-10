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

    public Task<Result> ValidateUserIdNotEmptyAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
            return Task.FromResult(Result.ValidationError("User ID cannot be null or empty."));

        return Task.FromResult(Result.Success());
    }

    public async Task<Result> ValidateUserExistsAsync(string userId)
    {
        var idResult = await ValidateUserIdNotEmptyAsync(userId);
        if (!idResult.IsSuccess)
            return idResult;

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.NotFound($"User with ID '{userId}' was not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateEmailUniqueAsync(string email)
    {
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser != null)
            return Result.Conflict("User with this email already exists.");

        return Result.Success();
    }

    public async Task<Result> ValidateForCreateAsync(CreateUserDto dto)
    {
        return await ValidateEmailUniqueAsync(dto.Email);
    }

    public async Task<Result> ValidateForUpdateAsync(UpdateUserDto dto)
    {
        return await ValidateUserExistsAsync(dto.Id);
    }

    public async Task<Result> ValidateForDeleteAsync(string userId)
    {
        return await ValidateUserExistsAsync(userId);
    }
}