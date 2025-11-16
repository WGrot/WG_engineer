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


    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ITwoFactorService twoFactorService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _twoFactorService = twoFactorService;
    }

    public async Task<Result> RegisterAsync(RegisterRequest request)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
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
                    TwoFactorEnabled = user.TwoFactorEnabled
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
            TwoFactorEnabled = user.TwoFactorEnabled
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
}