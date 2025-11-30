using RestaurantApp.Application.Interfaces;

namespace RestaurantApp.Api.Helpers;

public class LinkGenerator: ILinkGenerator
{
    private readonly IUrlHelper _urlHelper;

    public LinkGenerator(IUrlHelper urlHelper)
    {
        _urlHelper = urlHelper;
    }

    public string GeneratePasswordResetLink(string userId, string token)
    {
        return _urlHelper.GeneratePasswordResetLink(userId, token);
    }

    public string GenerateEmailConfirmationLink(string userId, string token)
    {
        return _urlHelper.GenerateEmailConfirmationLink(userId, token);
    }
}