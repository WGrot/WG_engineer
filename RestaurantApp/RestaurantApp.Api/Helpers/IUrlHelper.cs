namespace RestaurantApp.Api.Helpers;

public interface IUrlHelper
{
    string GenerateEmailConfirmationLink(string userId, string token);
    string GeneratePasswordResetLink(string userId, string token);
}