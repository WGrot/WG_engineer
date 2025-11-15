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


    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
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

        if (result.Succeeded)
        {
            var token = await _jwtService.GenerateJwtTokenAsync(user);
            var response = new LoginResponse
            {
                Token = token,
                ResponseUser = new ResponseUserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName
                }
            };
            return Result<LoginResponse>.Success(response);
        }

        return Result<LoginResponse>.Failure(result.ToString(), 401);
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