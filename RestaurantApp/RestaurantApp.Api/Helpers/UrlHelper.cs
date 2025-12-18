using System.Net;
using RestaurantApp.Application.Interfaces;

namespace RestaurantApp.Api.Helpers;

public class UrlHelper : IUrlHelper
{
    private readonly IConfiguration _configuration;
    
    public UrlHelper(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public string GenerateEmailConfirmationLink(string userId, string token)
    {
        var apiUrl = _configuration["AppURL:ApiUrl"];
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedUserId = WebUtility.UrlEncode(userId);
        return $"{apiUrl}/api/Auth/confirm-email?userId={encodedUserId}&token={encodedToken}";
    }

    public string GeneratePasswordResetLink(string userId, string token)
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var encodedUserId = WebUtility.UrlEncode(userId);
        var frontendUrl = _configuration["AppURL:FrontendUrl"];
        var resetLink = $"{frontendUrl}/reset-password?userId={encodedUserId}&token={encodedToken}";
        return resetLink;
    }

    public string GenerateInvitationLink(string token)
    {
        var encodedToken = WebUtility.UrlEncode(token);
        var frontendUrl = _configuration["AppURL:FrontendUrl"];
        return $"{frontendUrl}/invitations/accept?token={encodedToken}";
    }
}