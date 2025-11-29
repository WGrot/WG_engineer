using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services;

namespace RestaurantApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IReviewService, ReviewService>();

        return services;
    }
}