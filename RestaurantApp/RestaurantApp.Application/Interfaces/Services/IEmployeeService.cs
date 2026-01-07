using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Interfaces.Services;

public interface IEmployeeService
{
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct = default);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId, CancellationToken ct = default);
    Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto, CancellationToken ct = default);
    Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto, CancellationToken ct = default);
    Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto, CancellationToken ct = default);
    Task<Result> DeleteAsync(int id, CancellationToken ct = default);
    Task<Result> UpdateActiveStatusAsync(int id, bool isActive, CancellationToken ct = default);
}