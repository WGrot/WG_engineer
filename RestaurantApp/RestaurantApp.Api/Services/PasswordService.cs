using Microsoft.AspNetCore.Identity;
using RestaurantApp.Api.Helpers;
using RestaurantApp.Api.Services.Email;
using RestaurantApp.Api.Services.Email.Templates.AccountManagement;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;

namespace RestaurantApp.Api.Services;

public class PasswordService : IPasswordService
{
    
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUrlHelper _urlHelper;
    private readonly IEmailComposer _emailComposer;

    public PasswordService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUrlHelper urlHelper,
        IEmailComposer emailComposer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _urlHelper = urlHelper;
        _emailComposer = emailComposer;
    }
    
    public string GenerateSecurePassword(int length = 16)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = lowercase + uppercase + digits + special;
    
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
    
        var password = new char[length];
        password[0] = uppercase[bytes[0] % uppercase.Length];
        password[1] = lowercase[bytes[1] % lowercase.Length];
        password[2] = digits[bytes[2] % digits.Length];
        password[3] = special[bytes[3] % special.Length];
    
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[bytes[i] % allChars.Length];
        }
    
        return new string(password.OrderBy(x => Guid.NewGuid()).ToArray());
    }
    
    public async Task<Result> ForgotPasswordAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Result.Failure("Email is required", 400);
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            return Result.Success();
        }
        
        if (!user.EmailConfirmed)
        {
            return Result.Failure("Email is not confirmed", 400);
        }
        
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = _urlHelper.GeneratePasswordResetLink(user.Id, token);
        
        var emailBody = new ForgotPasswordEmail(user.UserName, resetLink);
        await _emailComposer.SendAsync(user.Email, emailBody);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
        {
            return Result.Failure("Invalid reset data", 400);
        }

        if (string.IsNullOrEmpty(request.UserId))
            return Result<ApplicationUser>.Failure("User ID is required", 400);
        
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return Result<ApplicationUser>.Failure("User not found", 404);
        
        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            var resetResult = Result.Failure("Password reset failed", 400);
            foreach (var error in result.Errors)
            {
                resetResult.Error += $"{error.Code}: {error.Description}\n";
            }
            return resetResult;
        }
        
        await _userManager.UpdateSecurityStampAsync(user);
        var emailBody = new PasswordResetEmail(user.UserName);
        await _emailComposer.SendAsync(user.Email, emailBody);

        return Result.Success();
    }
    
    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        if (string.IsNullOrEmpty(userId))
            return Result.Unauthorized("User not logged in");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Unauthorized("User not logged in");

        IdentityResult changeResult = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

        if (!changeResult.Succeeded)
        {
            var result = Result.Failure("Password change failed", 400);
            foreach (var error in changeResult.Errors)
                result.Error += $"{error.Code}: {error.Description}\n";
            return result;
        }

        await _signInManager.RefreshSignInAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        return Result.Success();
    }
}