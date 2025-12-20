using Microsoft.Extensions.Options;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Interfaces.Settings;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Services;

public class EmployeeInvitationService : IEmployeeInvitationService
{
    private readonly IEmployeeInvitationRepository _invitationRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly InvitationSettings _settings;
    private readonly IUserNotificationService _userNotificationService;
    private readonly IUrlHelper _urlHelper;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantPermissionRepository _restaurantPermissionRepository;

    public EmployeeInvitationService(
        IEmployeeInvitationRepository invitationRepository,
        IRestaurantRepository restaurantRepository,
        IRestaurantEmployeeRepository employeeRepository,
        IOptions<InvitationSettings> settings,
        ICurrentUserService currentUserService,
        IUserRepository userRepository,
        IRestaurantPermissionRepository restaurantPermissionRepository,
        IUserNotificationService userNotificationService,
        IUrlHelper urlHelper)
    {
        _invitationRepository = invitationRepository;
        _restaurantRepository = restaurantRepository;
        _employeeRepository = employeeRepository;
        _currentUserService = currentUserService;
        _settings = settings.Value;
        _userRepository = userRepository;
        _restaurantPermissionRepository = restaurantPermissionRepository;
        _userNotificationService = userNotificationService;
        _urlHelper = urlHelper;
    }

    public async Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        var user = await _userRepository.GetByEmailAsync(dto.Email);
        
        
        var invitation = new EmployeeInvitation
        {
            RestaurantId = dto.RestaurantId,
            UserId = user!.Id,
            SenderId = _currentUserService.UserId!,
            Role = dto.Role.ToDomain(),
            Token = TokenHelper.GenerateUrlToken(_settings.TokenLength),
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_settings.ExpirationHours)
        };

        var invitationLink = _urlHelper.GenerateInvitationLink(invitation.Token);
        
        var notification = await _userNotificationService.CreateAsync(new UserNotification()
        {
            UserId = user.Id,
            Title = "Restaurant Invitation",
            Content = $"You have been invited to join '{restaurant!.Name}' as a {dto.Role.ToString()}.",
            ActionUrl = invitationLink,
            Type = NotificationType.Info,
            Category = NotificationCategory.EmployeeInvitation, 
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        
        invitation.NotificationId = notification.Id;
        
        await _invitationRepository.AddAsync(invitation);

        return Result.Success(invitation.ToDto());
    }
    
    public async Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token)
    {
        var invitation = await ValidateTokenAsync(token);

        invitation!.Status = InvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation);

        var employee = new RestaurantEmployee
        {
            RestaurantId = invitation.RestaurantId,
            UserId = invitation.UserId,
            Role = invitation.Role,
            CreatedAt = DateTime.UtcNow,
            Permissions = new List<RestaurantPermission>(),
            IsActive = true
            
        };

        await _employeeRepository.AddAsync(employee);
        await _userNotificationService.DeleteAsync(invitation.NotificationId, invitation.UserId);
        
        await _userNotificationService.CreateAsync(new UserNotification()
        {
            UserId = invitation.SenderId,
            Title = "Restaurant Invitation",
            Content = $"User {invitation.User.UserName} accepted your invitation to work at restaurant {invitation.Restaurant.Name}.",
            Type = NotificationType.Success,
            Category = NotificationCategory.General, 
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });

        foreach (var permissionType in RolePermissionHelper.GetDefaultPermissions(invitation.Role))
        {
            employee.Permissions.Add(new RestaurantPermission
            {
                RestaurantEmployeeId = employee.Id,
                Permission = permissionType
            });
        }
        
        await _restaurantPermissionRepository.AddRangeAsync(employee.Permissions);

        return Result.Success(invitation.ToDto());
    }

    public async Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token)
    {
        var invitation = await ValidateTokenAsync(token);


        invitation!.Status = InvitationStatus.Rejected;
        invitation.RespondedAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation);
        
        await _userNotificationService.CreateAsync(new UserNotification()
        {
            UserId = invitation.SenderId,
            Title = "Restaurant Invitation",
            Content = $"User {invitation.User.UserName} rejected your invitation to work at restaurant {invitation.Restaurant.Name}.",
            Type = NotificationType.Warning,
            Category = NotificationCategory.General, 
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        
        await _userNotificationService.DeleteAsync(invitation.NotificationId, invitation.UserId);
        
        return Result.Success( invitation.ToDto());
    }

    public async Task<EmployeeInvitation?> ValidateTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var invitation = await _invitationRepository.GetByTokenAsync(token);

        if (invitation == null)
            return null;

        if (invitation.Status != InvitationStatus.Pending)
            return null;

        if (invitation.ExpiresAt < DateTime.UtcNow)
            return null;

        return invitation;
    }
}