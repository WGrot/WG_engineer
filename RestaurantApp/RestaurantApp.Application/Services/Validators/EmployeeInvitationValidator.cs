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


    public async Task<Result> ValidateForCreateAsync(CreateInvitationDto dto, CancellationToken ct)
    {
        var restaurant = await _restaurantRepository.GetByIdAsync(dto.RestaurantId, ct);
        if (restaurant == null)
            return Result.NotFound($"Restaurant with not found.");

        var user = await _userRepository.GetByEmailAsync(dto.Email, ct);
        if (user == null)
        {
            return Result.Failure($"No user with this email found.");
        }
        
        var existingEmployee = await _employeeRepository.GetByUserIdWithDetailsAsync(user.Id, ct);
        foreach (var employee in existingEmployee)
        {
            if (employee.RestaurantId == dto.RestaurantId)
                return Result.Failure($"User is already an employee of restaurant.");
        }
        
        var existingInvitation = await _employeeInvitationRepository.GetByUserIdAsync(user.Id, ct);
        foreach (var invitation in existingInvitation)
        {
            if (invitation.RestaurantId == dto.RestaurantId && invitation.Status == Domain.Enums.InvitationStatus.Pending)
                return Result.Failure($"User already received your invite.");
        }
        
        return Result.Success();
    }

    public async Task<Result> ValidateForAccept(string token, CancellationToken ct)
    {
        var invitation =  await _employeeInvitationRepository.GetByTokenAsync(token, ct);
        if (invitation == null)
            return Result.NotFound("Invitation not found.");
        if (invitation.Status != Domain.Enums.InvitationStatus.Pending)
            return Result.Failure("Invitation is no longer valid.");
        
        var user =  await _userRepository.GetByIdAsync(invitation.UserId, ct);

        if (user == null)
        {
            return Result.NotFound("User not found.");
        }
            
        return Result.Success();
        
    }
}