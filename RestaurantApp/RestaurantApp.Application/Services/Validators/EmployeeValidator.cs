using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Services.Validators;

public class EmployeeValidator : IEmployeeValidator
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantEmployeeRepository _employeeRepository;

    public EmployeeValidator(
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IRestaurantEmployeeRepository employeeRepository)
    {
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
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
        return await ValidateEmployeeExistsAsync(dto.Id);
    }

    public async Task<Result> ValidateEmployeeExistsAsync(int employeeId)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId);
        if (employee == null)
            return Result.NotFound($"Employee with ID {employeeId} not found.");

        return Result.Success();
    }
}