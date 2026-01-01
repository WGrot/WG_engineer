using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Permissions;

namespace RestaurantApp.Application.Interfaces.Validators;


public interface IRestaurantPermissionValidator
{
    Task<Result> ValidatePermissionExistsAsync(int permissionId, CancellationToken ct);
    Task<Result> ValidateEmployeeExistsAsync(int employeeId, CancellationToken ct);
    Task<Result> ValidateForCreateAsync(CreateRestaurantPermissionDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(RestaurantPermissionDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateEmployeePermissionsAsync(UpdateEmployeePermisionsDto dto, CancellationToken ct);
}