using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IEmployeeValidator
{
    Task<Result> ValidateForCreateAsync(CreateEmployeeDto dto);
    Task<Result> ValidateForUpdateAsync(UpdateEmployeeDto dto);
    Task<Result> ValidateEmployeeExistsAsync(int employeeId);
}