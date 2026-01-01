using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Interfaces.Services;


public interface IEmployeeInvitationService
{

    Task<Result<EmployeeInvitationDto>> CreateInvitationAsync(CreateInvitationDto dto, CancellationToken ct = default);
    Task<Result<EmployeeInvitationDto>> AcceptInvitationAsync(string token, CancellationToken ct = default);
    Task<Result<EmployeeInvitationDto>> RejectInvitationAsync(string token, CancellationToken ct = default);
    Task<EmployeeInvitation?> ValidateTokenAsync(string token, CancellationToken ct = default);
}