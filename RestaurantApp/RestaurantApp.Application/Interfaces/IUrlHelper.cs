namespace RestaurantApp.Application.Interfaces;

public interface IUrlHelper
{
    string GenerateEmailConfirmationLink(string userId, string token);
    string GeneratePasswordResetLink(string userId, string token);
    string GenerateInvitationLink(string token);
}