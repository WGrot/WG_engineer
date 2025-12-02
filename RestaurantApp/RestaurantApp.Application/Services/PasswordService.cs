using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services.Email.Templates.AccountManagement;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;

namespace RestaurantApp.Application.Services;

public class PasswordService: IPasswordService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ILinkGenerator _linkGenerator;
    private readonly IEmailComposer _emailComposer;

    public PasswordService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        ILinkGenerator linkGenerator,
        IEmailComposer emailComposer)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _linkGenerator = linkGenerator;
        _emailComposer = emailComposer;
    }

    public string GenerateSecurePassword(int length = 16)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = lowercase + uppercase + digits + special;

        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);

        var password = new char[length];
        
        password[0] = uppercase[bytes[0] % uppercase.Length];
        password[1] = lowercase[bytes[1] % lowercase.Length];
        password[2] = digits[bytes[2] % digits.Length];
        password[3] = special[bytes[3] % special.Length];

        for (var i = 4; i < length; i++)
        {
            password[i] = allChars[bytes[i] % allChars.Length];
        }
        
        return new string(password.OrderBy(_ => Guid.NewGuid()).ToArray());
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
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
        var resetLink = _linkGenerator.GeneratePasswordResetLink(user.Id, token);

        var emailBody = new ForgotPasswordEmail(user.UserName!, resetLink);
        await _emailComposer.SendAsync(user.Email!, emailBody);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
    {
        var validationResult = ValidateResetPasswordRequest(request);
        if (!validationResult.IsSuccess)
        {
            return validationResult;
        }

        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return Result.Failure("User not found", 404);
        }

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            return CreateIdentityErrorResult("Password reset failed", result.Errors);
        }

        await _userManager.UpdateSecurityStampAsync(user);
        
        var emailBody = new PasswordResetEmail(user.UserName!);
        await _emailComposer.SendAsync(user.Email!, emailBody);

        return Result.Success();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result.Unauthorized("User not logged in");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Unauthorized("User not logged in");
        }
        
        

        var changeResult = await _userManager.ChangePasswordAsync(
            user, 
            request.CurrentPassword, 
            request.NewPassword);

        if (!changeResult.Succeeded)
        {
            return CreateIdentityErrorResult("Password change failed", changeResult.Errors);
        }

        await _signInManager.RefreshSignInAsync(user);
        await _userManager.UpdateSecurityStampAsync(user);

        return Result.Success();
    }

    #region Private Methods

    private static Result ValidateResetPasswordRequest(ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserId))
        {
            return Result.Failure("User ID is required", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Token))
        {
            return Result.Failure("Reset token is required", 400);
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result.Failure("New password is required", 400);
        }

        return Result.Success();
    }

    private static Result CreateIdentityErrorResult(string message, IEnumerable<IdentityError> errors)
    {
        var errorDetails = string.Join(Environment.NewLine, 
            errors.Select(e => $"{e.Code}: {e.Description}"));
        
        return Result.Failure($"{message}: {errorDetails}", 400);
    }

    #endregion
}