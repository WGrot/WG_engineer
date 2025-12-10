using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IUserValidator
{
    Task<Result> ValidateUserExistsAsync(string userId, CancellationToken ct = default);
    Task<Result> ValidateUserIdNotEmptyAsync(string userId, CancellationToken ct = default);
    Task<Result> ValidateEmailUniqueAsync(string email, CancellationToken ct = default);
    Task<Result> ValidateForCreateAsync(CreateUserDto dto, CancellationToken ct = default);
    Task<Result> ValidateForUpdateAsync(UpdateUserDto dto, CancellationToken ct = default);
    Task<Result> ValidateForDeleteAsync(string userId, CancellationToken ct = default);
}