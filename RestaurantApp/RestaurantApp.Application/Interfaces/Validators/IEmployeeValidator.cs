using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IEmployeeValidator
{
    Task<Result> ValidateForCreateAsync(CreateEmployeeDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(UpdateEmployeeDto dto, CancellationToken ct);
    Task<Result> ValidateEmployeeExistsAsync(int employeeId, CancellationToken ct);
}