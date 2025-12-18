using Microsoft.Extensions.Options;
using RestaurantApp.Application.Helpers;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
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
    private readonly IUserNotificationService _userNotificationService;
    private readonly InvitationSettings _settings;
    private readonly IUserNotificationRepository _userNotificationRepository;
    private readonly IUrlHelper _urlHelper;

    public EmployeeInvitationService(
        IEmployeeInvitationRepository invitationRepository,
        IRestaurantRepository restaurantRepository,
        IRestaurantEmployeeRepository employeeRepository,
        IOptions<InvitationSettings> settings,
        IUserNotificationService userNotificationService,
        IUserNotificationRepository userNotificationRepository,
        IUrlHelper urlHelper)
    {
        _invitationRepository = invitationRepository;
        _restaurantRepository = restaurantRepository;
        _employeeRepository = employeeRepository;
        _userNotificationService = userNotificationService;
        _settings = settings.Value;
        _userNotificationRepository = userNotificationRepository;
        _urlHelper = urlHelper;
    }

    public async Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);

        var invitation = new EmployeeInvitation
        {
            RestaurantId = dto.RestaurantId,
            UserId = dto.UserId,
            Role = dto.Role.ToDomain(),
            Token = TokenHelper.GenerateUrlToken(_settings.TokenLength),
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_settings.ExpirationHours)
        };

        var invitationLink = _urlHelper.GenerateInvitationLink(invitation.Token);
        
        var notification = await _userNotificationRepository.AddAsync(new UserNotification()
        {
            UserId = dto.UserId,
            Title = "Restaurant Invitation",
            Content = $"You have been invited to join '{restaurant.Name}' as a {dto.Role.ToString()}.",
            ActionUrl = invitationLink,
            Type = NotificationType.Info,
            Category = NotificationCategory.EmployeeInvitation, 
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        
        invitation.NotificationId = notification!.Id;
        
        await _invitationRepository.AddAsync(invitation);

        return Result.Success(invitation.ToDto());
    }
    

    public async Task<Result> CancelInvitationAsync(int invitationId)
    {
        var invitation = await _invitationRepository.GetByIdAsync(invitationId)
            ?? throw new InvalidOperationException("Invitation not found.");

        // var isOwner = await _restaurantRepository.IsOwnerAsync(invitation.RestaurantId, ownerId);
        // if (!isOwner){}

        if (invitation.Status != InvitationStatus.Pending)
            throw new InvalidOperationException("Only pending invitations can be cancelled.");

        invitation.Status = InvitationStatus.Cancelled;
        invitation.RespondedAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation);

        // TODO: Optionally notify user that invitation was cancelled
        // await _emailService.SendInvitationCancelledEmailAsync(user.Email, restaurant.Name);
        
        return Result.Success();
    }

    public async Task<Result<IEnumerable<EmployeeInvitation>>> GetRestaurantInvitationsAsync(int restaurantId,
        string ownerId)
    {
        // var isOwner = await _restaurantRepository.IsOwnerAsync(restaurantId, ownerId);
        // if (!isOwner){}

        return Result.Success( await _invitationRepository.GetByRestaurantIdAsync(restaurantId));
    }

    public async Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token)
    {
        var invitation = await ValidateTokenAsync(token);
            // ?? throw new InvalidTokenException();

        // if (invitation.UserId != userId)
            // throw new UnauthorizedInvitationAccessException();

        // var isAlreadyEmployee = await _employeeRepository.ExistsAsync(invitation.RestaurantId, userId);
        // if (isAlreadyEmployee)
        //     throw new UserAlreadyEmployeeException();

        invitation.Status = InvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation);

        var employee = new RestaurantEmployee
        {
            RestaurantId = invitation.RestaurantId,
            UserId = invitation.UserId,
            Role = invitation.Role,
            
        };

        await _employeeRepository.AddAsync(employee);
        await _userNotificationRepository.DeleteAsync(invitation.NotificationId, invitation.UserId);
        // TODO: Notify restaurant owner about accepted invitation


        return Result.Success(invitation.ToDto());
    }

    public async Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token)
    {
        var invitation = await ValidateTokenAsync(token);


        invitation.Status = InvitationStatus.Rejected;
        invitation.RespondedAt = DateTime.UtcNow;

        await _invitationRepository.UpdateAsync(invitation);

        // TODO: Notify restaurant owner about rejected invitation
        // await _emailService.SendInvitationRejectedEmailAsync(owner.Email, user.Name, restaurant.Name);
        await _userNotificationRepository.DeleteAsync(invitation.NotificationId, invitation.UserId);
        
        return Result.Success( invitation.ToDto());
    }

    public async Task<Result<IEnumerable<EmployeeInvitation>>> GetUserPendingInvitationsAsync(string userId)
    {
        return Result.Success( (await _invitationRepository.GetPendingByUserIdAsync(userId)));
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