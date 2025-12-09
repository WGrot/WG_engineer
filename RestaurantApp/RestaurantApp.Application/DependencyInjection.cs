using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Application.Services;
using RestaurantApp.Application.Services.Decorators.Authorization;
using RestaurantApp.Application.Services.Decorators.Validation;
using RestaurantApp.Application.Services.Email;
using RestaurantApp.Application.Services.Validators;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.Validators.Employee;
using RestaurantApp.Shared.Validators.Menu;

namespace RestaurantApp.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateMenuCategoryDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateMenuCategoryDtoValidator>();
        services.AddScoped<IMenuCategoryValidator, MenuCategoryValidator>();
        
        services.AddScoped<MenuCategoryService>();
        services.AddScoped<IMenuCategoryService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuCategoryService>();
            
            var createValidator = sp.GetRequiredService<IValidator<CreateMenuCategoryDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<UpdateMenuCategoryDto>>();
            var businessValidator = sp.GetRequiredService<IMenuCategoryValidator>();
            var validatedService = new ValidatedMenuCategoryService(innerService, createValidator, updateValidator, businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();
    
            return new AuthorizedMenuCategoryService(validatedService, currentUser, permissionChecker);
        });
        
        services.AddScoped<MenuService>();
        services.AddScoped<IMenuService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuService(innerService, currentUser, permissionChecker);
        });
        
        
        services.AddScoped<IMenuItemValidator, MenuItemValidator>();

        services.AddScoped<MenuItemService>();
        services.AddScoped<IMenuItemService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuItemService>();
            
            var businessValidator = sp.GetRequiredService<IMenuItemValidator>();
            var validatedService = new ValidatedMenuItemService(innerService, businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuItemService(validatedService, currentUser, permissionChecker);
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
        
        services.AddValidatorsFromAssemblyContaining<CreateEmployeeDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateEmployeeDtoValidator>();
        services.AddScoped<IEmployeeValidator, EmployeeValidator>();
        
        services.AddScoped<EmployeeService>();
        services.AddScoped<IEmployeeService>(sp =>
        {
            var employeeService = sp.GetRequiredService<EmployeeService>();

            var validatedService = new ValidatedEmployeeService(
                employeeService,
                sp.GetRequiredService<IValidator<CreateEmployeeDto>>(),
                sp.GetRequiredService<IValidator<UpdateEmployeeDto>>(),
                sp.GetRequiredService<IEmployeeValidator>()
            );

            return new AuthorizedEmployeeService(
                validatedService,
                sp.GetRequiredService<ICurrentUserService>(),
                sp.GetRequiredService<IAuthorizationChecker>()
            );
        });
        
        services.AddScoped<ReviewService>();
        services.AddScoped<IReviewService>(sp =>
        {
            var innerService = sp.GetRequiredService<ReviewService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedReviewService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<UserService>();
        services.AddScoped<IUserService>(sp =>
        {
            var innerService = sp.GetRequiredService<UserService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedUserService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<RestaurantSettingsService>();
        services.AddScoped<IRestaurantSettingsService>(sp =>
        {
            var innerService = sp.GetRequiredService<RestaurantSettingsService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantSettingsService(innerService, currentUser, permissionChecker);
        });
        
        services.AddScoped<RestaurantImageService>();
        services.AddScoped<IRestaurantImageService>(sp =>
        {
            var innerService = sp.GetRequiredService<RestaurantImageService>();
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantImageService(innerService, currentUser, permissionChecker);
        });
        
        services.AddTransient<IEmailComposer, EmailComposer>();
        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IRestaurantSearchService, RestaurantSearchService>();
        services.AddScoped<IRestaurantOpeningHoursService, RestaurantOpeningHoursService>();
        services.AddScoped<IRestaurantDashboardService, RestaurantDashboardService>();
        services.AddScoped<ITableAvailabilityService, TableAvailabilityService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        
        services.AddScoped<ITableReservationService, TableReservationService>();
        return services;
    }
}