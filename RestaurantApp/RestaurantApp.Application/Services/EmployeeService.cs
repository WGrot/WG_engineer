using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class EmployeeService : IEmployeeService
{
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly IUserRepository _userRepository;

    public EmployeeService(
        IRestaurantEmployeeRepository employeeRepository,
        IUserRepository userRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync()
    {
        var employees = await _employeeRepository.GetAllWithDetailsAsync();
        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(employees.ToDtoList());
    }

    public async Task<Result<RestaurantEmployeeDto>> GetByIdAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdWithDetailsAsync(id);

        return employee == null
            ? Result<RestaurantEmployeeDto>.NotFound($"Employee with ID {id} not found.")
            : Result<RestaurantEmployeeDto>.Success(employee.ToDto());
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId)
    {
        var employees = await _employeeRepository.GetByRestaurantIdWithDetailsAsync(restaurantId);
        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(employees.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId)
    {
        var employees = await _employeeRepository.GetByUserIdWithDetailsAsync(userId);
        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(employees.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId)
    {
        var employees = await _employeeRepository.GetByRestaurantIdWithDetailsAsync(restaurantId);

        var userIds = employees.Select(e => e.UserId).Distinct();
        var users = await _userRepository.GetByIdsAsync(userIds);
        var usersDict = users.ToDictionary(u => u.Id);

        var dtoList = employees
            .Where(e => usersDict.ContainsKey(e.UserId))
            .Select(e => MapToEmployeeDto(e, usersDict[e.UserId]))
            .ToList();

        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(dtoList);
    }

    public async Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        employee.Role = newRoleEnumDto.ToDomain();
        await _employeeRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        var employee = new RestaurantEmployee
        {
            UserId = dto.UserId,
            RestaurantId = dto.RestaurantId,
            Role = dto.RoleEnumDto.ToDomain(),
            Permissions = new List<RestaurantPermission>(),
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        await _employeeRepository.AddAsync(employee);
        await _employeeRepository.SaveChangesAsync();

        var createdEmployee = await _employeeRepository.GetByIdWithDetailsAsync(employee.Id);
        return Result<RestaurantEmployeeDto>.Success(createdEmployee!.ToDto());
    }

    public async Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.Id);
        
        employee!.Role = dto.RoleEnumDto.ToDomain();
        employee.IsActive = dto.IsActive;

        await _employeeRepository.SaveChangesAsync();

        var updatedEmployee = await _employeeRepository.GetByIdWithDetailsAsync(employee.Id);
        return Result<RestaurantEmployeeDto>.Success(updatedEmployee!.ToDto());
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            return Result.NotFound($"Employee with ID {id} not found.");

        _employeeRepository.Remove(employee);
        await _employeeRepository.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive)
    {
        var employee = await _employeeRepository.GetByIdAsync(id);
        if (employee == null)
            return Result.NotFound($"Employee with ID {id} not found.");

        employee.IsActive = isActive;
        await _employeeRepository.SaveChangesAsync();

        return Result.Success();
    }

    private static RestaurantEmployeeDto MapToEmployeeDto(RestaurantEmployee employee, ApplicationUser user)
    {
        var dto = employee.ToDto();
        dto.Email = user.Email ?? string.Empty;
        dto.FirstName = user.FirstName ?? string.Empty;
        dto.LastName = user.LastName ?? string.Empty;
        dto.PhoneNumber = user.PhoneNumber ?? string.Empty;
        return dto;
    }
}