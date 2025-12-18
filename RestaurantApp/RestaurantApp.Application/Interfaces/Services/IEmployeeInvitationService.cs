using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Interfaces.Services;


public interface IEmployeeInvitationService
{

    Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto);
    Task<Result> CancelInvitationAsync(int invitationId);
    Task<Result<IEnumerable<EmployeeInvitation>>> GetRestaurantInvitationsAsync(int restaurantId, string ownerId);
    

    Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token);
    Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token);
    Task<Result<IEnumerable<EmployeeInvitation>>> GetUserPendingInvitationsAsync(string userId);

    Task<EmployeeInvitation?> ValidateTokenAsync(string token);
}