namespace RestaurantApp.Application.Interfaces;

public interface ILinkGenerator
{
    string GeneratePasswordResetLink(string userId, string token);
    string GenerateEmailConfirmationLink(string userId, string token);
}