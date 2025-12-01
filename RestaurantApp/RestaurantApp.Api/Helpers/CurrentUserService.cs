using System.Security.Claims;
using RestaurantApp.Application.Interfaces;

namespace RestaurantApp.Api.Helpers;

public class CurrentUserService: ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public bool IsAuthenticated => !string.IsNullOrEmpty(UserId);
}