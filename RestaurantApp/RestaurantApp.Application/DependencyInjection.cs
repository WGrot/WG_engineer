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
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Menu;
using RestaurantApp.Shared.DTOs.Menu.Categories;
using RestaurantApp.Shared.DTOs.Menu.Tags;
using RestaurantApp.Shared.DTOs.Menu.Variants;
using RestaurantApp.Shared.DTOs.Permissions;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Review;
using RestaurantApp.Shared.DTOs.Tables;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Validators.Employee;
using RestaurantApp.Shared.Validators.Menu;
using RestaurantApp.Shared.Validators.Permission;
using RestaurantApp.Shared.Validators.Reservation;
using RestaurantApp.Shared.Validators.Reservations;
using RestaurantApp.Shared.Validators.Review;
using RestaurantApp.Shared.Validators.Table;
using RestaurantApp.Shared.Validators.User;

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
        
        services.AddValidatorsFromAssemblyContaining<CreateMenuDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateMenuDtoValidator>();
        services.AddScoped<IMenuValidator, MenuValidator>();


        services.AddScoped<MenuService>();
        services.AddScoped<IMenuService>(sp =>
        {

            var innerService = sp.GetRequiredService<MenuService>();
            
            var createValidator = sp.GetRequiredService<IValidator<CreateMenuDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<UpdateMenuDto>>();
            var businessValidator = sp.GetRequiredService<IMenuValidator>();
            var validatedService = new ValidatedMenuService(innerService, createValidator, updateValidator, businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuService(validatedService, currentUser, permissionChecker);
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
        
        services.AddValidatorsFromAssemblyContaining<CreateMenuItemTagDtoValidator>();
        services.AddScoped<IMenuItemTagValidator, MenuItemTagValidator>();
        
        services.AddScoped<MenuItemTagService>();
        services.AddScoped<IMenuItemTagService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuItemTagService>();
            
            var createValidator = sp.GetRequiredService<IValidator<CreateMenuItemTagDto>>();
            var businessValidator = sp.GetRequiredService<IMenuItemTagValidator>();
            var validatedService = new ValidatedMenuItemTagService(innerService, createValidator, businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuItemTagService(validatedService, currentUser, permissionChecker);
        });


        
        services.AddValidatorsFromAssemblyContaining<MenuItemVariantDtoValidator>();
        services.AddScoped<IMenuItemVariantValidator, MenuItemVariantValidator>();
        
        services.AddScoped<MenuItemVariantService>();
        services.AddScoped<IMenuItemVariantService>(sp =>
        {
            var innerService = sp.GetRequiredService<MenuItemVariantService>();

            var validator = sp.GetRequiredService<IValidator<MenuItemVariantDto>>();
            var businessValidator = sp.GetRequiredService<IMenuItemVariantValidator>();

            var validatedService = new ValidatedMenuItemVariantService(
                innerService,
                validator,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedMenuItemVariantService(validatedService, currentUser, permissionChecker);
        });
        

        services.AddValidatorsFromAssemblyContaining<CreateRestaurantPermissionDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<RestaurantPermissionDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateEmployeePermissionsDtoValidator>();
        
        services.AddScoped<IRestaurantPermissionValidator, RestaurantPermissionValidator>();
        
        services.AddScoped<RestaurantPermissionService>();
        services.AddScoped<IRestaurantPermissionService>(sp =>
        {
            var innerService = sp.GetRequiredService<RestaurantPermissionService>();
            
            var createValidator = sp.GetRequiredService<IValidator<CreateRestaurantPermissionDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<RestaurantPermissionDto>>();
            var updateEmployeePermissionsValidator = sp.GetRequiredService<IValidator<UpdateEmployeePermisionsDto>>();
            var businessValidator = sp.GetRequiredService<IRestaurantPermissionValidator>();

            var validatedService = new ValidatedRestaurantPermissionService(
                innerService,
                createValidator,
                updateValidator,
                updateEmployeePermissionsValidator,
                businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantPermissionService(validatedService, currentUser, permissionChecker);
        });
        
        services.AddValidatorsFromAssemblyContaining<CreateTableDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateTableDtoValidator>();
        services.AddScoped<ITableValidator, TableValidator>();


        services.AddScoped<TableService>();
        services.AddScoped<ITableService>(sp =>
        {
            var innerService = sp.GetRequiredService<TableService>();

            var createValidator = sp.GetRequiredService<IValidator<CreateTableDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<UpdateTableDto>>();
            var businessValidator = sp.GetRequiredService<ITableValidator>();

            var validatedService = new ValidatedTableService(
                innerService,
                createValidator,
                updateValidator,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedTableService(validatedService, currentUser, permissionChecker);
        });
        
        services.AddValidatorsFromAssemblyContaining<ReservationDtoValidator>();
        services.AddScoped<IReservationValidator, ReservationValidator>();
        
        services.AddScoped<ReservationService>();
        services.AddScoped<IReservationService>(sp =>
        {
            var innerService = sp.GetRequiredService<ReservationService>();

            var validator = sp.GetRequiredService<IValidator<ReservationDto>>();
            var businessValidator = sp.GetRequiredService<IReservationValidator>();

            var validatedService = new ValidatedReservationService(
                innerService,
                validator,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedReservationService(validatedService, currentUser, permissionChecker);
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
        
        services.AddValidatorsFromAssemblyContaining<CreateReviewDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateReviewDtoValidator>();
        services.AddScoped<IReviewValidator, ReviewValidator>();


        services.AddScoped<ReviewService>();
        services.AddScoped<IReviewService>(sp =>
        {
            var innerService = sp.GetRequiredService<ReviewService>();

            var createValidator = sp.GetRequiredService<IValidator<CreateReviewDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<UpdateReviewDto>>();
            var businessValidator = sp.GetRequiredService<IReviewValidator>();

            var validatedService = new ValidatedReviewService(
                innerService,
                createValidator,
                updateValidator,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedReviewService(validatedService, currentUser, permissionChecker);
        });
        
        services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
        services.AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>();
        services.AddScoped<IUserValidator, UserValidator>();
        
        services.AddScoped<UserService>();
        services.AddScoped<IUserService>(sp =>
        {
            var innerService = sp.GetRequiredService<UserService>();

            var createValidator = sp.GetRequiredService<IValidator<CreateUserDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<UpdateUserDto>>();
            var businessValidator = sp.GetRequiredService<IUserValidator>();

            var validatedService = new ValidatedUserService(
                innerService,
                createValidator,
                updateValidator,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedUserService(validatedService, currentUser, permissionChecker);
        });
        
        services.AddScoped<IRestaurantSettingsValidator, RestaurantSettingsValidator>();
        
        services.AddScoped<RestaurantSettingsService>();
        services.AddScoped<IRestaurantSettingsService>(sp =>
        {
            var innerService = sp.GetRequiredService<RestaurantSettingsService>();
            var businessValidator = sp.GetRequiredService<IRestaurantSettingsValidator>();

            var validatedService = new ValidatedRestaurantSettingsService(
                innerService,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantSettingsService(validatedService, currentUser, permissionChecker);
        });
        
        
        services.AddScoped<IRestaurantImageValidator, RestaurantImageValidator>();
        services.AddScoped<RestaurantImageService>();
        services.AddScoped<IRestaurantImageService>(sp =>
        {

            var innerService = sp.GetRequiredService<RestaurantImageService>();
            
            var businessValidator = sp.GetRequiredService<IRestaurantImageValidator>();
            var validatedService = new ValidatedRestaurantImageService(innerService, businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantImageService(validatedService, currentUser, permissionChecker);
        });
        
        
        services.AddValidatorsFromAssemblyContaining<RestaurantBasicInfoDto>();
        services.AddValidatorsFromAssemblyContaining<CreateRestaurantDto>();
        
        services.AddScoped<IRestaurantValidator, RestaurantValidator>();
        
        services.AddScoped<RestaurantService>();
        services.AddScoped<IRestaurantService>(sp =>
        {
            var innerService = sp.GetRequiredService<RestaurantService>();


            var createRestaurantValidator = sp.GetRequiredService<IValidator<CreateRestaurantDto>>();
            var basicInfoValidator = sp.GetRequiredService<IValidator<RestaurantBasicInfoDto>>();
            var businessValidator = sp.GetRequiredService<IRestaurantValidator>();

            var validatedService = new ValidatedRestaurantService(
                innerService,
                createRestaurantValidator,
                basicInfoValidator,
                businessValidator);
            
            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedRestaurantService(validatedService, currentUser, permissionChecker);
        });
        
        services.AddValidatorsFromAssemblyContaining<CreateTableReservationDtoValidator>();
        services.AddScoped<ITableReservationValidator, TableReservationValidator>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        
        services.AddScoped<TableReservationService>();
        services.AddScoped<ITableReservationService>(sp =>
        {
            var innerService = sp.GetRequiredService<TableReservationService>();

            var createValidator = sp.GetRequiredService<IValidator<CreateTableReservationDto>>();
            var updateValidator = sp.GetRequiredService<IValidator<TableReservationDto>>();
            var businessValidator = sp.GetRequiredService<ITableReservationValidator>();
    
            var validatedService = new ValidatedTableReservationService(
                innerService,
                createValidator,
                updateValidator,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedTableReservationService(validatedService, currentUser, permissionChecker);
        });
        
        
        

        services.AddScoped<IEmployeeInvitationValidator, EmployeeInvitationValidator>();
        services.AddScoped<EmployeeInvitationService>();
        services.AddScoped<IEmployeeInvitationService>(sp =>
        {
            var innerService = sp.GetRequiredService<EmployeeInvitationService>();
            
            var businessValidator = sp.GetRequiredService<IEmployeeInvitationValidator>();
    
            var validatedService = new ValidatedInvitationService(
                innerService,
                businessValidator);

            var currentUser = sp.GetRequiredService<ICurrentUserService>();
            var permissionChecker = sp.GetRequiredService<IAuthorizationChecker>();

            return new AuthorizedInvitationService(validatedService, currentUser, permissionChecker);
        });
        
        
        services.AddTransient<IEmailComposer, EmailComposer>();
        services.AddScoped<IRestaurantSearchService, RestaurantSearchService>();
        services.AddScoped<IRestaurantOpeningHoursService, RestaurantOpeningHoursService>();
        services.AddScoped<IRestaurantDashboardService, RestaurantDashboardService>();
        services.AddScoped<ITableAvailabilityService, TableAvailabilityService>();
        services.AddScoped<IPasswordService, PasswordService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IAuthService, AuthService>();
        
        
        

        services.AddScoped<IUserNotificationService, UserNotificationService>();

        return services;
    }
}