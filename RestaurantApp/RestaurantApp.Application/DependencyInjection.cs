using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services;
using RestaurantApp.Application.Services.Email;

namespace RestaurantApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IMenuItemService, MenuItemService>();
        services.AddScoped<IMenuItemTagService, MenuItemTagService>();
        services.AddScoped<IMenuCategoryService, MenuCategoryService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddTransient<IEmailComposer, EmailComposer>();

        
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IRestaurantSearchService, RestaurantSearchService>();
        services.AddScoped<IRestaurantOpeningHoursService, RestaurantOpeningHoursService>();
        services.AddScoped<IRestaurantDashboardService, RestaurantDashboardService>();
        return services;
    }
}