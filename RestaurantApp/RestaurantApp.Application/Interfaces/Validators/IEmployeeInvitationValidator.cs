using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IEmployeeInvitationValidator
{
    Task<Result> ValidateForCreateAsync(CreateInvitationDto dto, CancellationToken ct);
    
    Task<Result> ValidateForAccept(string token, CancellationToken ct);
    
}