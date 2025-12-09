using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IRestaurantPermissionValidator
{
    Task<Result> ValidatePermissionExistsAsync(int permissionId, CancellationToken ct = default);
    Task<Result> ValidateEmployeeExistsAsync(int employeeId, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(RestaurantPermissionDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct = default);
}