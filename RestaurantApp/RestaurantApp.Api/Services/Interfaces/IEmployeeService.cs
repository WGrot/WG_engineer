using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IEmployeeService
{
    Task<Result<IEnumerable<RestaurantEmployee>>> GetAllAsync();
    Task<Result<RestaurantEmployee>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RestaurantEmployee>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<IEnumerable<RestaurantEmployee>>> GetByUserIdAsync(string userId);

    // Nowa metoda zwracająca od razu DTO
    Task<Result<IEnumerable<ResponseRestaurantEmployeeDto>>> GetEmployeesByRestaurantDtoAsync(int restaurantId);
    
    Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRole newRole);

    Task<Result<RestaurantEmployee>> CreateAsync(CreateEmployeeDto dto);
    Task<Result<RestaurantEmployee>> UpdateAsync(int id, UpdateEmployeeDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> UpdateActiveStatusAsync(int id, bool isActive);
}