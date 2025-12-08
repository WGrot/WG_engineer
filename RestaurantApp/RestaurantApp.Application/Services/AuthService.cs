using System.IdentityModel.Tokens.Jwt;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services.Email.Templates.AccountManagement;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Auth;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Services;

public class AuthService: IAuthService
{
    private readonly IUserRepository _userRepository;      
    private readonly IIdentityService _identityService;      
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IEmailComposer _emailComposer;
    private readonly ILinkGenerator _emailLinkGenerator;
    private readonly ITokenBlacklistService _tokenBlacklistService;
    

    public AuthService(
        IUserRepository userRepository,
        IIdentityService identityService,
        ITokenService tokenService,
        ITwoFactorService twoFactorService,
        IEmailComposer emailComposer,
        ILinkGenerator emailLinkGenerator,
        ITokenBlacklistService tokenBlacklistService)
    {
        _userRepository = userRepository;
        _identityService = identityService;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
        _emailComposer = emailComposer;
        _emailLinkGenerator = emailLinkGenerator;
        _tokenBlacklistService = tokenBlacklistService;
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
        
        var createResult = await _identityService.CreateUserAsync(user, request.Password);
        if (createResult.IsFailure)
            return Result.Failure(createResult.Error, createResult.StatusCode);

        var createdUser = createResult.Value!;
        
        var token = await _identityService.GenerateEmailConfirmationTokenAsync(createdUser);
        var confirmationLink = _emailLinkGenerator.GenerateEmailConfirmationLink(createdUser.Id, token);
        
        var email = new AccountRegisteredEmail(createdUser.FirstName, confirmationLink);
        await _emailComposer.SendAsync(createdUser.Email!, email);

        return Result.Success();
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, string ipAddress)
    {

        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
            return Result<LoginResponse>.Unauthorized("Incorrect email or password");

        var passwordValid = await _identityService.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
            return Result<LoginResponse>.Failure("Incorrect email or password", 401);
        
        if (user.TwoFactorEnabled)
        {
            var twoFactorResult = await HandleTwoFactorAsync(user, request.TwoFactorCode);
            if (twoFactorResult.IsFailure)
                return twoFactorResult;
            
            if (twoFactorResult.Value?.RequiresTwoFactor == true)
                return twoFactorResult;
        }

        var (accessToken, refreshToken, refreshExpiresAt) = 
            await _tokenService.GenerateTokensAsync(user, user.TwoFactorEnabled, ipAddress);

        var response = new LoginResponse
        {
            Token = accessToken,
            RefreshToken = refreshToken,
            RefreshExpiresAt = refreshExpiresAt,
            RequiresTwoFactor = false,
            ResponseUser = MapToLoginDto(user)
        };

        return Result<LoginResponse>.Success(response);
    }

    public async Task<Result> LogoutAsync(string? refreshToken, string? accessToken, string ipAddress)
    {
        // Blacklist access token
        if (!string.IsNullOrEmpty(accessToken))
        {
            await BlacklistAccessTokenAsync(accessToken);
        }

        // Revoke refresh token
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress, "logout");
        }

        return Result.Success();
    }
    
    
    private async Task BlacklistAccessTokenAsync(string accessToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(accessToken);

            var jti = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
            var exp = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Exp)?.Value;

            if (jti == null || exp == null)
                return;

            var expiryDate = DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).UtcDateTime;
            var ttl = expiryDate - DateTime.UtcNow;

            if (ttl > TimeSpan.Zero)
            {
                await _tokenBlacklistService.BlacklistTokenAsync(jti, ttl);
            }
        }
        catch
        {

        }
    }

    public async Task<Result<ResponseUserDto>> GetCurrentUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Result<ResponseUserDto>.Failure("User ID is required", 400);
        
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result<ResponseUserDto>.Failure("User not found", 404);

        return Result.Success(MapToDto(user));
    }

    public async Task<Result<List<ResponseUserDto>>> GetAllUsersAsync()
    {
        var users = await _userRepository.SearchAsync(null, null, null, null, null);
        var userDtos = users.Select(MapToDto).ToList();
        return Result<List<ResponseUserDto>>.Success(userDtos);
    }

    public async Task<Result> ConfirmEmailAsync(string userId, string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return Result.Failure("Incorrect verification data", 400);

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            return Result.Failure("User does not exist", 404);

        if (user.EmailConfirmed)
            return Result.Success();
        
        return await _identityService.ConfirmEmailAsync(user, token);
    }

    public async Task<Result> ResendEmailConfirmationAsync(string email)
    {
        if (string.IsNullOrEmpty(email))
            return Result.Failure("Email is required", 400);

        var user = await _userRepository.GetByEmailAsync(email);
        if (user == null)
            return Result.Success();

        if (user.EmailConfirmed)
            return Result.Failure("Email is already confirmed", 400);

        var token = await _identityService.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = _emailLinkGenerator.GenerateEmailConfirmationLink(user.Id, token);
        
        var emailBody = new EmailConfirmationEmail(user.UserName!, confirmationLink);
        await _emailComposer.SendAsync(user.Email!, emailBody);

        return Result.Success();
    }

    #region Private Methods

    private async Task<Result<LoginResponse>> HandleTwoFactorAsync(ApplicationUser user, string? twoFactorCode)
    {
        if (string.IsNullOrEmpty(twoFactorCode))
        {
            return Result<LoginResponse>.Success(new LoginResponse
            {
                RequiresTwoFactor = true,
                ResponseUser = MapToLoginDto(user)
            });
        }
        
        if (await _identityService.IsLockedOutAsync(user))
            return Result<LoginResponse>.Unauthorized("Account is locked due to multiple failed attempts");

        if (string.IsNullOrEmpty(user.TwoFactorSecretKey))
            return Result<LoginResponse>.Failure("2FA is not properly configured", 500);

        bool isValid = _twoFactorService.ValidateCode(user.TwoFactorSecretKey, twoFactorCode);
        if (!isValid)
        {
            await _identityService.RecordFailedAccessAsync(user);
            return Result<LoginResponse>.Unauthorized("Invalid 2FA code");
        }

        await _identityService.ResetAccessFailedCountAsync(user);
        return Result<LoginResponse>.Success(new LoginResponse { RequiresTwoFactor = false });
    }

    private static ResponseUserDto MapToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        PhoneNumber = user.PhoneNumber
    };

    private static ResponseUserLoginDto MapToLoginDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        TwoFactorEnabled = user.TwoFactorEnabled,
        EmailVerified = user.EmailConfirmed
    };

    #endregion
}