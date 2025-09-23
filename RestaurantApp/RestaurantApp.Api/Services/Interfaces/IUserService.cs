using RestaurantApp.Api.Models.DTOs;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IUserService
{
    Task<ResponseUserDto> GetByIdAsync(string id);
}