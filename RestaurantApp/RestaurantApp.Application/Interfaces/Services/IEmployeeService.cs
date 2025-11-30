using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync();
    Task<Result<RestaurantEmployeeDto>> GetByIdAsync(int id);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId);
    Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto);
    Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto);
    Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto);
    Task<Result> DeleteAsync(int id);
    Task<Result> UpdateActiveStatusAsync(int id, bool isActive);
}