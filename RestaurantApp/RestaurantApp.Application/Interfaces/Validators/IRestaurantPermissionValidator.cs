using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IRestaurantPermissionValidator
{
    Task<Result> ValidatePermissionExistsAsync(int permissionId);
    Task<Result> ValidateEmployeeExistsAsync(int employeeId);
    Task<Result> ValidateForCreateAsync(CreateRestaurantPermissionDto dto);
    Task<Result> ValidateForUpdateAsync(RestaurantPermissionDto dto);
    Task<Result> ValidateForUpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto);
}