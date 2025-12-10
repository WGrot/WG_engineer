using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Application.Interfaces.Validators;

public interface IUserValidator
{
    Task<Result> ValidateUserExistsAsync(string userId);
    Task<Result> ValidateUserIdNotEmptyAsync(string userId);
    Task<Result> ValidateEmailUniqueAsync(string email);
    Task<Result> ValidateForCreateAsync(CreateUserDto dto);
    Task<Result> ValidateForUpdateAsync(UpdateUserDto dto);
    Task<Result> ValidateForDeleteAsync(string userId);
}