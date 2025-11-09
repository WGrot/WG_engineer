using RestaurantApp.Api.Common;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IEmployeeService
{
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync();
    Task<Result<RestaurantEmployeeDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId);

    // Nowa metoda zwracająca od razu DTO
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantDtoAsync(int restaurantId);
    
    Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRole newRole);

    Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto);
    Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> UpdateActiveStatusAsync(int id, bool isActive);
}