using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Common;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IUserService
{
    Task<Result<ResponseUserDto>> GetByIdAsync(string id);
    Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName, string? phoneNumber, string? email);
    Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto);
}