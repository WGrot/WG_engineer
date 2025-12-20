using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Interfaces.Services;


public interface IEmployeeInvitationService
{

    Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto);
    Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token);
    Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token);
    Task<EmployeeInvitation?> ValidateTokenAsync(string token);
}