using Microsoft.AspNetCore.Identity;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services.Email.Templates.AccountManagement;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Application.Services;

public class UserService: IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeService _employeeService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailComposer _emailComposer;

    public UserService(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager,
        IEmployeeService employeeService,
        IPasswordService passwordService,
        IEmailComposer emailComposer)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _employeeService = employeeService;
        _passwordService = passwordService;
        _emailComposer = emailComposer;
    }

    public async Task<Result<ResponseUserDto>> GetByIdAsync(string id, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result<ResponseUserDto>.ValidationError("User ID cannot be null or empty");
        }

        var user = await _userRepository.GetByIdAsync(id, ct);

        if (user == null)
        {
            return Result<ResponseUserDto>.NotFound($"User with ID '{id}' was not found");
        }

        return Result<ResponseUserDto>.Success(MapToResponseDto(user));
    }

    public async Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(
        string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount,
        CancellationToken ct = default)
    {
        try
        {
            var users = await _userRepository.SearchAsync(firstName, lastName, phoneNumber, email, amount, ct);
            var responseUserDtos = users.Select(MapToResponseDto).ToList();

            return Result<IEnumerable<ResponseUserDto>>.Success(responseUserDtos);
        }
        catch (Exception ex)
        {
            return Result<IEnumerable<ResponseUserDto>>.InternalError(
                $"An error occurred while searching users: {ex.Message}");
        }
    }

    public async Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userDto.Email))
        {
            return Result<CreateUserDto>.ValidationError("Email is required");
        }

        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            return Result<CreateUserDto>.Conflict("User with this email already exists");
        }

        var generatedPassword = _passwordService.GenerateSecurePassword();

        var user = new ApplicationUser
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            PhoneNumber = userDto.PhoneNumber,
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, generatedPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result<CreateUserDto>.InternalError($"Failed to create user: {errors}");
        }

        if (userDto.RestaurantId.HasValue)
        {
            var employeeResult = await CreateEmployeeAssociationAsync(user, userDto);
            if (!employeeResult.IsSuccess)
            {
                await _userManager.DeleteAsync(user);
                return Result<CreateUserDto>.InternalError(employeeResult.Error ?? "Failed to create employee association");
            }
        }

        var resultDto = new CreateUserDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            RestaurantId = userDto.RestaurantId,
            Password = generatedPassword
        };

        await SendWelcomeEmailAsync(user, generatedPassword);

        return Result<CreateUserDto>.Created(resultDto);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto, CancellationToken ct = default)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);

        if (user == null)
        {
            return Result.Failure("User not found", 404);
        }

        UpdateUserFromDto(user, dto);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(errors);
        }

        return Result.Success();
    }

    public async Task<Result> DeleteUserAsync(string userId, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return Result.Failure("User ID is required", 400);
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return Result.Failure("User not found", 404);
        }

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(Environment.NewLine, 
                result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            return Result.Failure($"Failed to delete user: {errors}", 400);
        }

        return Result.Success();
    }

    #region Private Methods

    private static ResponseUserDto MapToResponseDto(ApplicationUser user)
    {
        return new ResponseUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };
    }

    private static void UpdateUserFromDto(ApplicationUser user, UpdateUserDto dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.FirstName))
            user.FirstName = dto.FirstName;
        
        if (!string.IsNullOrWhiteSpace(dto.LastName))
            user.LastName = dto.LastName;
        
        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            user.PhoneNumber = dto.PhoneNumber;
        
        if (!string.IsNullOrWhiteSpace(dto.Email))
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }
    }

    private async Task<Result> CreateEmployeeAssociationAsync(ApplicationUser user, CreateUserDto userDto)
    {
        try
        {
            var employee = new CreateEmployeeDto
            {
                UserId = user.Id,
                RestaurantId = userDto.RestaurantId!.Value,
                RoleEnumDto = userDto.Role ?? RestaurantRoleEnumDto.Employee,
            };

            await _employeeService.CreateAsync(employee);
            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to create employee association: {ex.Message}");
        }
    }

    private async Task SendWelcomeEmailAsync(ApplicationUser user, string password)
    {
        var emailBody = new EmployeeAccontCreated(user.FirstName, password);
        await _emailComposer.SendAsync(user.Email!, emailBody);
    }

    #endregion
}