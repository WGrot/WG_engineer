using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Validators;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;

namespace RestaurantApp.Application.Services.Validators;

public class EmployeeInvitationValidator : IEmployeeInvitationValidator
{
    private readonly IRestaurantRepository _restaurantRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRestaurantEmployeeRepository _employeeRepository;
    private readonly IEmployeeInvitationRepository _employeeInvitationRepository;

    public EmployeeInvitationValidator(
        IRestaurantRepository restaurantRepository,
        IUserRepository userRepository,
        IRestaurantEmployeeRepository employeeRepository,
        IEmployeeInvitationRepository employeeInvitationRepository)
    {
        _restaurantRepository = restaurantRepository;
        _userRepository = userRepository;
        _employeeRepository = employeeRepository;
        _employeeInvitationRepository = employeeInvitationRepository;
    }


    public async Task<Result> ValidateForCreateAsync(CreateInvitationDto dto)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with not found.");

        var user = await _userRepository.GetByEmailAsync(dto.Email);
        if (user == null)
        {
            return Result.Failure($"No user with this email found.");
        }
        
        var existingEmployee = await _employeeRepository.GetByUserIdWithDetailsAsync(user.Id);
        foreach (var employee in existingEmployee)
        {
            if (employee.RestaurantId == dto.RestaurantId)
                return Result.Failure($"User is already an employee of restaurant.");
        }
        
        var existingInvitation = await _employeeInvitationRepository.GetByUserIdAsync(user.Id);
        foreach (var invitation in existingInvitation)
        {
            if (invitation.RestaurantId == dto.RestaurantId && invitation.Status == Domain.Enums.InvitationStatus.Pending)
                return Result.Failure($"User already received your invite.");
        }
        
        return Result.Success();
    }

    public async Task<Result> ValidateForAccept(string token)
    {
        var invitation =  await _employeeInvitationRepository.GetByTokenAsync(token);
        if (invitation == null)
            return Result.NotFound("Invitation not found.");
        if (invitation.Status != Domain.Enums.InvitationStatus.Pending)
            return Result.Failure("Invitation is no longer valid.");
        
        var user =  await _userRepository.GetByIdAsync(invitation.UserId);

        if (user == null)
        {
            return Result.NotFound("User not found.");
        }
            
        return Result.Success();
        
    }
}