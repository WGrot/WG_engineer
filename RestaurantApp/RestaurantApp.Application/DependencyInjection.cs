using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services;
using RestaurantApp.Application.Services.Email;
using RestaurantApp.Domain.Models;

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
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IMenuItemVariantService, MenuItemVariantService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<ITableAvailabilityService, TableAvailabilityService>();
        services.AddScoped<IRestaurantImageService, RestaurantImageService>();
        services.AddScoped<IRestaurantPermissionService, RestaurantPermissionService>();
        services.AddScoped<IRestaurantSettingsService, RestaurantSettingsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<ITableReservationService, TableReservationService>();
        return services;
    }
}