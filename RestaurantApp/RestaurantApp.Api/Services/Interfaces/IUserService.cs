using RestaurantApp.Api.Functional;
using RestaurantApp.Api.Models.DTOs;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IUserService
{
    Task<Either<ApiError, ResponseUserDto>> GetByIdAsync(string id);

    Task<Either<ApiError, IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName, string? phoneNumber, string? email);
    
    Task<Either<ApiError, CreateUserDto>> CreateAsync(CreateUserDto userDto);
}