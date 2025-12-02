using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services;
using RestaurantApp.Application.Services.Decorators.Authorization;
using RestaurantApp.Application.Services.Email;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<MenuCategoryService>();
        services.AddScoped<IMenuCategoryService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuCategoryService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuCategoryService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<MenuService>();
        services.AddScoped<IMenuService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuService(innerService, currentUser, permissionChecker);
        });
        
        
        services.AddScoped<MenuItemService>();
        services.AddScoped<IMenuItemService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuItemService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuItemService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<MenuItemTagService>();
        services.AddScoped<IMenuItemTagService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuItemTagService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuItemTagService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<MenuItemVariantService>();
        services.AddScoped<IMenuItemVariantService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuItemVariantService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuItemVariantService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<RestaurantPermissionService>();
        services.AddScoped<IRestaurantPermissionService>(sp =>
        {
            var innerService = sp.GetRequiredService<RestaurantPermissionService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantPermissionService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<TableService>();
        services.AddScoped<ITableService>(sp =>
        {
            var innerService = sp.GetRequiredService<TableService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedTableService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<ReservationService>();
        services.AddScoped<IReservationService>(sp =>
        {
            var innerService = sp.GetRequiredService<ReservationService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedReservationService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<IReviewService, ReviewService>();
        services.AddTransient<IEmailComposer, EmailComposer>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IRestaurantSearchService, RestaurantSearchService>();
        services.AddScoped<IRestaurantOpeningHoursService, RestaurantOpeningHoursService>();
        services.AddScoped<IRestaurantDashboardService, RestaurantDashboardService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<ITableAvailabilityService, TableAvailabilityService>();
        services.AddScoped<IRestaurantImageService, RestaurantImageService>();
        services.AddScoped<IRestaurantSettingsService, RestaurantSettingsService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddScoped<ITableReservationService, TableReservationService>();
        return services;
    }
}