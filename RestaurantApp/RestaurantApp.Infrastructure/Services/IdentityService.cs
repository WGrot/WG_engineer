using Microsoft.AspNetCore.Identity;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Infrastructure.Services;

public class IdentityService: IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;

    public IdentityService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<Result<ApplicationUser>> CreateUserAsync(ApplicationUser user, string password)
    {
        var result = await _userManager.CreateAsync(user, password);
        
        if (result.Succeeded)
            return Result<ApplicationUser>.Success(user);
            
        return Result<ApplicationUser>.Failure(FormatErrors(result), 400);
    }

    public async Task<bool> CheckPasswordAsync(ApplicationUser user, string password)
    {
        return await _userManager.CheckPasswordAsync(user, password);
    }

    public async Task<Result> ChangePasswordAsync(
        ApplicationUser user, 
        string currentPassword, 
        string newPassword)
    {
        var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        return ToResult(result);
    }

    public async Task<string> GenerateEmailConfirmationTokenAsync(ApplicationUser user)
    {
        return await _userManager.GenerateEmailConfirmationTokenAsync(user);
    }

    public async Task<Result> ConfirmEmailAsync(ApplicationUser user, string token)
    {
        var result = await _userManager.ConfirmEmailAsync(user, token);
        return ToResult(result);
    }

    public async Task<string> GeneratePasswordResetTokenAsync(ApplicationUser user)
    {
        return await _userManager.GeneratePasswordResetTokenAsync(user);
    }

    public async Task<Result> ResetPasswordAsync(ApplicationUser user, string token, string newPassword)
    {
        var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
        return ToResult(result);
    }

    public async Task<bool> IsLockedOutAsync(ApplicationUser user)
    {
        return await _userManager.IsLockedOutAsync(user);
    }

    public async Task RecordFailedAccessAsync(ApplicationUser user)
    {
        await _userManager.AccessFailedAsync(user);
    }

    public async Task ResetAccessFailedCountAsync(ApplicationUser user)
    {
        await _userManager.ResetAccessFailedCountAsync(user);
    }

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    public async Task<Result> AddToRoleAsync(ApplicationUser user, string role)
    {
        var result = await _userManager.AddToRoleAsync(user, role);
        return ToResult(result);
    }

    #region Private Helpers

    private static Result ToResult(IdentityResult identityResult)
    {
        return identityResult.Succeeded 
            ? Result.Success() 
            : Result.Failure(FormatErrors(identityResult), 400);
    }

    private static string FormatErrors(IdentityResult result)
    {
        return string.Join("; ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
    }

    #endregion
}