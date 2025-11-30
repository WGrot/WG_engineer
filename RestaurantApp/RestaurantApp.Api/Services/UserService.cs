using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Application.Interfaces;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Application.Services.Email.Templates.AccountManagement;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class UserService : IUserService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeService _employeeService;
    private readonly IPasswordService _passwordService;
    private readonly IEmailComposer _emailComposer;
    
    public UserService(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmployeeService employeeService, IPasswordService passwordService, IEmailComposer emailComposer)
    {
        _context = context;
        _userManager = userManager;
        _employeeService = employeeService;
        _passwordService = passwordService;
        _emailComposer = emailComposer;
    }

    public async Task<Result<ResponseUserDto>> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return Result<ResponseUserDto>.ValidationError("User ID cannot be null or empty");
        }

        var user = await _context.Users.FindAsync(id);
    
        if (user == null)
        {
            return Result<ResponseUserDto>.NotFound($"User with ID '{id}' was not found");
        }

        var responseUserDto = new ResponseUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };

        return Result<ResponseUserDto>.Success(responseUserDto);
    }

    public async Task<Result<IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName,
        string? phoneNumber, string? email, int? amount)
    {
        try
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(firstName))
            {
                query = query.Where(u => u.FirstName.ToLower().Contains(firstName.ToLower()));
            }
        
            if (!string.IsNullOrWhiteSpace(lastName))
            {
                query = query.Where(u => u.LastName.ToLower().Contains(lastName.ToLower()));
            }
        
            if (!string.IsNullOrWhiteSpace(phoneNumber))
            {
                query = query.Where(u => u.PhoneNumber.Contains(phoneNumber));
            }
        
            if (!string.IsNullOrWhiteSpace(email))
            {
                query = query.Where(u => u.Email.ToLower().Contains(email.ToLower()));
            }
            
            var takeAmount = amount.HasValue && amount.Value > 0 ? amount.Value : 50;
            query = query.Take(takeAmount);

            var users = await query.ToListAsync();
            
            var responseUserDtos = users.Select(user => new ResponseUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            }).ToList();
            
            return Result<IEnumerable<ResponseUserDto>>.Success(responseUserDtos);
        }
        catch (Exception ex)
        {
            // Logowanie błędu tutaj (jeśli używasz ILogger)
            return Result<IEnumerable<ResponseUserDto>>.InternalError($"An error occurred while searching users: {ex.Message}");
        }
    }

    public async Task<Result<CreateUserDto>> CreateAsync(CreateUserDto userDto)
    {
        // Walidacja
        if (string.IsNullOrWhiteSpace(userDto.Email))
        {
            return Result<CreateUserDto>.ValidationError("Email is required");
        }

        // Sprawdź czy użytkownik już istnieje
        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
        if (existingUser != null)
        {
            return Result<CreateUserDto>.Conflict("User with this email already exists");
        }

        // Wygeneruj hasło
        string generatedPassword = _passwordService.GenerateSecurePassword();

        // Stwórz użytkownika
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

        // Jeśli jest RestaurantId, dodaj jako pracownika
        if (userDto.RestaurantId.HasValue)
        {
            try
            {
                var employee = new CreateEmployeeDto
                {
                    UserId = user.Id,
                    RestaurantId = userDto.RestaurantId.Value,
                    RoleEnumDto = userDto.Role ?? RestaurantRoleEnumDto.Employee,
                };
        
                await _employeeService.CreateAsync(employee);
            }
            catch (Exception ex)
            {
                // Jeśli nie udało się utworzyć pracownika, usuń utworzonego użytkownika
                await _userManager.DeleteAsync(user);
                return Result<CreateUserDto>.InternalError($"Failed to create employee association: {ex.Message}");
            }
        }

        // Zwróć DTO z wygenerowanym hasłem
        var resultDto = new CreateUserDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            RestaurantId = userDto.RestaurantId,
            Password = generatedPassword
        };

        var emailBody = new EmployeeAccontCreated(user.FirstName, generatedPassword);
        
        await _emailComposer.SendAsync( user.Email ,emailBody);
        
        return Result<CreateUserDto>.Created(resultDto);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id);

        if (user == null)
            throw new Exception("User not found");
        
        user.UpdateFromDto(dto);

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return Result.Failure(errors);
        }
        return Result.Success();
    }
    
    
    public async Task<Result> DeleteUserAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
            return Result<ApplicationUser>.Failure("User ID is required", 400);
        
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return Result<ApplicationUser>.Failure("User not found", 404);

        var result = await _userManager.DeleteAsync(user);

        if (!result.Succeeded)
        {
            var deleteResult = Result.Failure("Failed to delete user", 400);
            foreach (var error in result.Errors)
            {
                deleteResult.Error += $"{error.Code}: {error.Description}\n";
            }

            return deleteResult;
        }

        return Result.Success();
    }
}