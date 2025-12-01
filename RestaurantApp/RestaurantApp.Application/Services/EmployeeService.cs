using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Mappers;
using RestaurantApp.Application.Mappers.EnumMappers;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class EmployeeService: IEmployeeService
{
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;

    public EmployeeService(
        IRestaurantEmployeeRepository employeeRepository,
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository)
    {
        _employeeRepository = employeeRepository;
        _restaurantRepository = restaurantRepository;
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
        var dtoList = new List<RestaurantEmployeeDto>();

        foreach (var employee in employees)
        {
            var user = await _userRepository.GetByIdAsync(employee.UserId);
            if (user == null)
                continue;

            var dto = new RestaurantEmployeeDto
            {
                Id = employee.Id,
                UserId = employee.UserId,
                RestaurantId = employee.RestaurantId,
                Restaurant = employee.Restaurant.ToDto(),
                RoleEnumDto = employee.Role.ToShared(),
                Permissions = employee.Permissions.ToDtoList(),
                CreatedAt = employee.CreatedAt,
                IsActive = employee.IsActive,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            };

            dtoList.Add(dto);
        }

        return Result<IEnumerable<RestaurantEmployeeDto>>.Success(dtoList);
    }

    public async Task<Result> UpdateEmployeeRoleAsync(int employeeId, RestaurantRoleEnumDto newRoleEnumDto)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
        {
            return Result.NotFound($"Employee with ID {employeeId} not found.");
        }

        employee.Role = newRoleEnumDto.ToDomain();
        await _employeeRepository.SaveChangesAsync();
        
        return Result.Success();
    }

    public async Task<Result<RestaurantEmployeeDto>> CreateAsync(CreateEmployeeDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        if (restaurant == null)
            return Result<RestaurantEmployeeDto>.NotFound($"Restaurant with ID {dto.RestaurantId} not found.");

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
        if (employee == null)
            return Result<RestaurantEmployeeDto>.NotFound($"Employee with ID {dto.Id} not found.");

        employee.Role = dto.RoleEnumDto.ToDomain();
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
}