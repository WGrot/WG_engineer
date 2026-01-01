using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IUserValidator
{
    Task<Result> ValidateUserExistsAsync(string userId, CancellationToken ct);
    Task<Result> ValidateUserIdNotEmptyAsync(string userId, CancellationToken ct);
    Task<Result> ValidateEmailUniqueAsync(string email, CancellationToken ct);
    Task<Result> ValidateForCreateAsync(CreateUserDto dto, CancellationToken ct);
    Task<Result> ValidateForUpdateAsync(UpdateUserDto dto, CancellationToken ct);
    Task<Result> ValidateForDeleteAsync(string userId, CancellationToken ct);
}