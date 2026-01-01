using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services.Validators;

public class EmployeeValidator : IEmployeeValidator
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly ICurrentUserService _currentUserService;

    public EmployeeValidator(
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IRestaurantEmployeeRepository employeeRepository,
        ICurrentUserService currentUserService)
    {
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result> ValidateForCreateAsync(CreateEmployeeDto dto, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId, ct);
        if (user == null)
            return Result.NotFound($"User not found.");

        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId, ct);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with not found.");

        var existingEmployee = await _employeeRepository.GetByUserIdWithDetailsAsync(dto.UserId, ct);
        foreach (var employee in existingEmployee)
        {
            if (employee.RestaurantId == dto.RestaurantId)
                return Result.Failure($"User is already an employee of restaurant.");
        }
        
        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(UpdateEmployeeDto dto, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.Id, ct);
        if (employee == null)
            return Result.NotFound($"Employee with ID {dto.Id} not found.");

        if (dto.RoleEnumDto == RestaurantRoleEnumDto.Owner)
        {
            var currentUserEmployee = await _employeeRepository.GetByUserIdWithDetailsAsync(_currentUserService.UserId!, ct);
            var restaurantUser = currentUserEmployee.FirstOrDefault(e => e.RestaurantId == employee.RestaurantId);
            if(restaurantUser == null || restaurantUser.Role != RestaurantRole.Owner)
                return Result.Failure("Only the owner can assign the owner role to another employee.");
        }
        
        return Result.Success();
    }

    public async Task<Result> ValidateEmployeeExistsAsync(int employeeId, CancellationToken ct)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, ct);
        if (employee == null)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        return Result.Success();
    }
}