using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Api.Services.Interfaces;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtService _jwtService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;


    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ITwoFactorService twoFactorService,
        IEmailService emailService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _twoFactorService = twoFactorService;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = false
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Zakoduj token do URL
            var encodedToken = WebUtility.UrlEncode(token);
            var encodedUserId = WebUtility.UrlEncode(user.Id);

            var apiUrl = _configuration["AppURL:ApiUrl"];
            var confirmationLink = $"{apiUrl}/api/Auth/confirm-email?userId={encodedUserId}&token={encodedToken}";

            // Przygotuj treść emaila
            var emailBody = $@"""
                <h2>Hello {user.FirstName}!</h2>
                <p>Thank you for registering in DineOps, Click this link to verify your email:</p>
                <p><a href='{confirmationLink}'>Confirm email</a></p>
                <p>If you didn't sign in to our site ignore this message</p>
            """;

            // Wyślij email
            await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

            return Result.Success();
        }

        Result registerResult = Result.Failure("Rejestracja nie powiodła się\n", 400);
        foreach (var error in result.Errors)
        {
            registerResult.Error += error.Code + error.Description + "\n";
        }

        return registerResult;
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return Result<LoginResponse>.Unauthorized($"Incorrect email or password");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

        if (!result.Succeeded)
        {
            return Result<LoginResponse>.Failure("Incorrect email or password", 401);
        }

        if (user.TwoFactorEnabled)
        {
            if (string.IsNullOrEmpty(request.TwoFactorCode))
            {
                var responseWithout2FA = new LoginResponse
                {
                    RequiresTwoFactor = true,
                    ResponseUser = new ResponseUserLoginDto()
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        TwoFactorEnabled = user.TwoFactorEnabled,
                        EmailVerified = user.EmailConfirmed
                    }
                };
                return Result<LoginResponse>.Success(responseWithout2FA);
            }

            // Sprawdź blokadę konta
            if (await _userManager.IsLockedOutAsync(user))
            {
                return Result<LoginResponse>.Unauthorized("Account is locked due to multiple failed attempts");
            }

            if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            {
                return Result<LoginResponse>.Failure("2FA is not properly configured", 500);
            }

            bool isValid = _twoFactorService.ValidateCode(user.TwoFactorSecretKey, request.TwoFactorCode);

            if (!isValid)
            {
                // Zwiększ licznik nieudanych prób
                await _userManager.AccessFailedAsync(user);

                return Result<LoginResponse>.Unauthorized("Invalid 2FA code");
            }

            // Zresetuj licznik po udanej weryfikacji
            await _userManager.ResetAccessFailedCountAsync(user);
        }

        var token = await _jwtService.GenerateJwtTokenAsync(user, user.TwoFactorEnabled);

        var response = new LoginResponse
        {
            Token = token,
            RequiresTwoFactor = false,
            ResponseUser = new ResponseUserLoginDto()
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TwoFactorEnabled = user.TwoFactorEnabled,
                EmailVerified = user.EmailConfirmed
            }
        };

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result> LogoutAsync()
    {
        await _signInManager.SignOutAsync();
        return Result.Success();
    }

    public async Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Result<ResponseUserDto>.Unauthorized("User not looged in");
        }

        if (string.IsNullOrEmpty(userId))
        {
            return Result<ResponseUserDto>.Unauthorized($"User not looged in");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result<ResponseUserDto>.Unauthorized($"User not looged in");
        }

        var response = new ResponseUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        };

        return Result.Success(response);
    }

    public async Task<Result<List<ResponseUserDto>>> GetAllUsersAsync()
    {
        var users = await _userManager.Users.ToListAsync();

        var userDtos = users.Select(user => new ResponseUserDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber
        }).ToList();

        return Result<List<ResponseUserDto>>.Success(userDtos);
    }

    public Result GetDebugAuthInfo(bool isAuthenticated, string authenticationType, IEnumerable<ClaimDto> claims)
    {
        throw new NotImplementedException();
    }

    public async Task<Result> ChangePasswordAsync(string userId, ChangePasswordRequest request)
    {
        if (string.IsNullOrEmpty(userId))
            return Result.Unauthorized("User not logged in");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result.Unauthorized("User not logged in");

        IdentityResult changeResult = await _userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword
        );

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

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return Result.Failure("Nieprawidłowe dane weryfikacyjne", 400);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("Użytkownik nie istnieje", 404);
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            var failureResult = Result.Failure("Weryfikacja nie powiodła się", 400);
            foreach (var error in result.Errors)
            {
                failureResult.Error += $"{error.Code}: {error.Description}\n";
            }

            return failureResult;
        }

        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure("User ID is required", 400);
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return Result.Failure("User not found", 404);
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var deleteResult = Result.Failure("Failed to delete user", 400);
            foreach (var error in result.Errors)
            {
                deleteResult.Error += $"{error.Code}: {error.Description}\n";
            }

            return deleteResult;
        }

        return Result.Success();
    }

    public async Task<Result> ResendEmailConfirmationAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return Result.Failure("Email is required", 400);
        }

        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Ze względów bezpieczeństwa zwracamy sukces, żeby nie ujawniać czy email istnieje
            return Result.Success();
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure("Email is already confirmed", 400);
        }

        // Wygeneruj nowy token weryfikacyjny
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedUserId = WebUtility.UrlEncode(user.Id);

        // Pobierz URL z konfiguracji
        var apiUrl = _configuration["AppURL:ApiUrl"];
        var confirmationLink = $"{apiUrl}/api/Auth/confirm-email?userId={encodedUserId}&token={encodedToken}";

        // Przygotuj treść emaila
        var emailBody = $@"
            <h2>Hello {user.FirstName}!</h2>
            <p>TO confirm your email in DineOps click this link</p>
            <p><a href='{confirmationLink}'>Potwierdź email</a></p>
            <p>If you didn't sign in to our site ignore this message.</p>
        ";

        // Wyślij email
        await _emailService.SendEmailAsync(user.Email, "Confirm email address", emailBody);

        return Result.Success();
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
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedUserId = WebUtility.UrlEncode(user.Id);

        // Utwórz link do frontendu (nie API!)
        var frontendUrl = _configuration["AppURL:FrontendUrl"];
        var resetLink = $"{frontendUrl}/reset-password?userId={encodedUserId}&token={encodedToken}";

        // Przygotuj treść emaila
        var emailBody = $@"
        <h2>Hello {user.FirstName}!</h2>
        <p>You requested to reset your password in DineOps.</p>
        <p><a href='{resetLink}'>Reset Password</a></p>
        <p>This link will expire in 24 hours.</p>
        <p>If you didn't request a password reset, please ignore this message.</p>
    ";

        // Wyślij email
        await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token))
        {
            return Result.Failure("Invalid reset data", 400);
        }

        var user = await _userManager.FindByIdAsync(request.UserId);

        if (user == null)
        {
            return Result.Failure("User not found", 404);
        }

        // Zresetuj hasło używając tokenu
        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);

        if (!result.Succeeded)
        {
            var resetResult = Result.Failure("Password reset failed", 400);
            foreach (var error in result.Errors)
            {
                resetResult.Error += $"{error.Code}: {error.Description}\n";
            }

            return resetResult;
        }

        // Opcjonalnie: wyloguj użytkownika ze wszystkich sesji
        await _userManager.UpdateSecurityStampAsync(user);

        // Opcjonalnie: wyślij email potwierdzający zmianę hasła
        var emailBody = $@"
        <h2>Hello {user.FirstName}!</h2>
        <p>Your password has been successfully changed.</p>
        <p>If you didn't make this change, please contact support immediately.</p>
    ";

        await _emailService.SendEmailAsync(user.Email, "Password Changed Successfully", emailBody);

        return Result.Success();
    }
}