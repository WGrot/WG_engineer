using RestaurantApp.Api.Common;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IUserService
{
    Task<Result<ResponseUserDto>> GetByIdAsync(string id);
    Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName, string? phoneNumber,
        string? email, int? amount);
    Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto);
    
    Task<Result> UpdateUserAsync(UpdateUserDto userDto);
    
    Task<Result> DeleteUserAsync(string userId);
}