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

    public async Task<Result> ValidateForCreateAsync(CreateEmployeeDto dto)
    {
        var user = await _userRepository.GetByIdAsync(dto.UserId);
        if (user == null)
            return Result.NotFound($"User with ID {dto.UserId} not found.");

        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with ID {dto.RestaurantId} not found.");

        return Result.Success();
    }

    public async Task<Result> ValidateForUpdateAsync(UpdateEmployeeDto dto)
    {
        var employee = await _employeeRepository.GetByIdAsync(dto.Id);
        if (employee == null)
            return Result.NotFound($"Employee with ID {dto.Id} not found.");

        if (dto.RoleEnumDto == RestaurantRoleEnumDto.Owner)
        {
            var currentUserEmployee = await _employeeRepository.GetByUserIdWithDetailsAsync(_currentUserService.UserId!);
            var restaurantUser = currentUserEmployee.FirstOrDefault(e => e.RestaurantId == employee.RestaurantId);
            if(restaurantUser == null || restaurantUser.Role != RestaurantRole.Owner)
                return Result.Failure("Only the owner can assign the owner role to another employee.");
        }
        
        return Result.Success();
    }

    public async Task<Result> ValidateEmployeeExistsAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        return Result.Success();
    }
}