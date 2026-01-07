using RestaurantApp.Application.Helpers;
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
    private readonly IRestaurantPermissionRepository _restaurantPermissionRepository;
    private readonly IUserRepository _userRepository;

    public EmployeeService(
        IRestaurantEmployeeRepository employeeRepository,
        IUserRepository userRepository,
        IRestaurantPermissionRepository restaurantPermissionRepository)
    {
        _employeeRepository = employeeRepository;
        _userRepository = userRepository;
        _restaurantPermissionRepository = restaurantPermissionRepository;
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetAllAsync(CancellationToken ct)
    {
        var employees = await _employeeRepository.GetAllWithDetailsAsync(ct);
        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(employees.ToDtoList());
    }
    

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        var employees = await _employeeRepository.GetByRestaurantIdWithDetailsAsync(restaurantId, ct);
        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(employees.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        var employees = await _employeeRepository.GetByUserIdWithDetailsAsync(userId, ct);
        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(employees.ToDtoList());
    }

    public async Task<Result<IEnumerable<RestaurantEmployeeDto>>> GetEmployeesByRestaurantWithUserDetailsAsync(int restaurantId, CancellationToken ct)
    {
        var employees = await _employeeRepository.GetByRestaurantIdWithDetailsAsync(restaurantId, ct);

        var userIds = employees.Select(e => e.UserId).Distinct();
        var users = await _userRepository.GetByIdsAsync(userIds, ct);
        var usersDict = users.ToDictionary(u => u.Id);

        var dtoList = employees
            .Where(e => usersDict.ContainsKey(e.UserId))
            .Select(e => MapToEmployeeDto(e, usersDict[e.UserId]))
            .ToList();

        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(dtoList);
    }

    public async Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, ct);
        if (employee == null)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        employee.Role = newRoleEnumDto.ToDomain();
        await _employeeRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto, CancellationToken ct)
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
        
        await _employeeRepository.AddAsync(employee, ct);
        await _employeeRepository.SaveChangesAsync(ct);

        var createdEmployee = await _employeeRepository.GetByIdWithDetailsAsync(employee.Id, ct);
        if (createdEmployee == null)
        {
            return Result<RestaurantEmployeeDto>.Failure("Failed to retrieve the created employee.");
        }
        
        foreach (var permissionType in RolePermissionHelper.GetDefaultPermissions(dto.RoleEnumDto.ToDomain()))
        {
            createdEmployee.Permissions.Add(new RestaurantPermission
            {
                RestaurantEmployeeId = createdEmployee.Id,
                Permission = permissionType
            });
        }
        
        await _restaurantPermissionRepository.AddRangeAsync(createdEmployee.Permissions, ct);
        return Result<RestaurantEmployeeDto>.Success(createdEmployee.ToDto());
    }

    public async Task<Result<RestaurantEmployeeDto>> UpdateAsync(UpdateEmployeeDto dto, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.Id, ct);
        
        employee!.Role = dto.RoleEnumDto.ToDomain();
        employee.IsActive = dto.IsActive;

        await _employeeRepository.SaveChangesAsync(ct);

        var updatedEmployee = await _employeeRepository.GetByIdWithDetailsAsync(employee.Id, ct);
        return Result<RestaurantEmployeeDto>.Success(updatedEmployee!.ToDto());
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, ct);
        if (employee == null)
            return Result.NotFound($"Employee with ID {id} not found.");

        _employeeRepository.Remove(employee, ct);
        await _employeeRepository.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<Result> UpdateActiveStatusAsync(int id, bool isActive, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(id, ct);
        if (employee == null)
            return Result.NotFound($"Employee with ID {id} not found.");

        employee.IsActive = isActive;
        await _employeeRepository.SaveChangesAsync(ct);

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