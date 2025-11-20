using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Helpers;
using RestaurantApp.Api.Services.Email;
using RestaurantApp.Api.Services.Email.Templates.AccountManagement;
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
    private readonly IUrlHelper _urlHelper;
    private readonly IEmailComposer _emailComposer;
    private readonly ITokenService _tokenService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtService jwtService,
        ITwoFactorService twoFactorService,
        IEmailService emailService,
        IConfiguration configuration,
        IUrlHelper urlHelper,
        IEmailComposer emailComposer,
        ITokenService tokenService,
        IHttpContextAccessor httpContextAccessor)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtService = jwtService;
        _twoFactorService = twoFactorService;
        _emailService = emailService;
        _configuration = configuration;
        _urlHelper = urlHelper;
        _emailComposer = emailComposer;
        _tokenService = tokenService;
        _httpContextAccessor = httpContextAccessor;
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
            var confirmationLink = _urlHelper.GenerateEmailConfirmationLink(user.Id, token);
            
            var email = new AccountRegisteredEmail(user.FirstName, confirmationLink);
            await _emailComposer.SendAsync(user.Email, email);

            return Result.Success();
        }

        Result registerResult = Result.Failure("Error during registration", 400);
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
        return Result<LoginResponse>.Unauthorized("Incorrect email or password");

    var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
    if (!result.Succeeded)
        return Result<LoginResponse>.Failure("Incorrect email or password", 401);

    if (user.TwoFactorEnabled)
    {
        if (string.IsNullOrEmpty(request.TwoFactorCode))
        {
            var responseWithout2FA = new LoginResponse
            {
                RequiresTwoFactor = true,
                ResponseUser = new ResponseUserLoginDto
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

        if (await _userManager.IsLockedOutAsync(user))
            return Result<LoginResponse>.Unauthorized("Account is locked due to multiple failed attempts");

        if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            return Result<LoginResponse>.Failure("2FA is not properly configured", 500);

        bool isValid = _twoFactorService.ValidateCode(user.TwoFactorSecretKey, request.TwoFactorCode);
        if (!isValid)
        {
            await _userManager.AccessFailedAsync(user);
            return Result<LoginResponse>.Unauthorized("Invalid 2FA code");
        }

        await _userManager.ResetAccessFailedCountAsync(user);
    }

    // Generujemy access + refresh (ale refresh token wyłącznie jako string — cookie ustawi kontroler)
    var ip = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
    var (accessToken, refreshToken, refreshExpiresAt) = await _tokenService.GenerateTokensAsync(user, user.TwoFactorEnabled, ip);

    var response = new LoginResponse
    {
        Token = accessToken,
        // nie przechowujemy refresh w serwisie; controller ustawi cookie
        RequiresTwoFactor = false,
        ResponseUser = new ResponseUserLoginDto
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            TwoFactorEnabled = user.TwoFactorEnabled,
            EmailVerified = user.EmailConfirmed
        },
        // dodatkowe pola dla convenience
        RefreshToken = refreshToken,
        RefreshExpiresAt = refreshExpiresAt
    };

    return Result<LoginResponse>.Success(response);
}

    public async Task<Result> LogoutAsync(string? presentedRefreshToken)
    {
        if (!string.IsNullOrEmpty(presentedRefreshToken))
        {
            var ip = _httpContextAccessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "unknown";
            await _tokenService.RevokeRefreshTokenAsync(presentedRefreshToken, ip, "logout");
        }
        await _signInManager.SignOutAsync();
        return Result.Success();
    }

    public async Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId)
    {
        var validationResult = await ValidateUserAsync(userId);
        if (validationResult.IsFailure)
        {
            return Result.Failure<ResponseUserDto>(validationResult.Error);
        }
        var user = validationResult.Value!;

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
    



    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
        {
            return Result.Failure("Incorrect verification data", 400);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User does not exist", 404);
        }

        if (user.EmailConfirmed)
        {
            return Result.Success();
        }

        var result = await _userManager.ConfirmEmailAsync(user, token);

        if (!result.Succeeded)
        {
            var failureResult = Result.Failure("Email verification failed", 400);
            foreach (var error in result.Errors)
            {
                failureResult.Error += $"{error.Code}: {error.Description}\n";
            }

            return failureResult;
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
            return Result.Success();
        }

        if (user.EmailConfirmed)
        {
            return Result.Failure("Email is already confirmed", 400);
        }
        
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = _urlHelper.GenerateEmailConfirmationLink(user.Id, token);
        var emailBody = new EmailConfirmationEmail(user.UserName, confirmationLink);
        await _emailComposer.SendAsync(user.Email, emailBody);

        return Result.Success();
    }



    
    private async Task<Result<ApplicationUser>> ValidateUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Result<ApplicationUser>.Failure("User ID is required", 400);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<ApplicationUser>.Failure("User not found", 404);
        
        return Result.Success(user);
    }
}

