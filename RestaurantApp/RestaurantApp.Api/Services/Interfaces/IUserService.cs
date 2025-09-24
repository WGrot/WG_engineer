using RestaurantApp.Api.Models.DTOs;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IUserService
{
    Task<ResponseUserDto> GetByIdAsync(string id);
    Task<IEnumerable<ResponseUserDto>> SearchAsync(string? firstName, string? lastName, string? phoneNumber, string email);
    
    Task<CreateUserDto> CreateAsync(CreateUserDto userDto);
}