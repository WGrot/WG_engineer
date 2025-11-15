using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Mappers;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Employees;
using RestaurantApp.Shared.DTOs.Users;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class UserService : IUserService
{
    private readonly ApiDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeService _employeeService;
    
    public UserService(ApiDbContext context, UserManager<ApplicationUser> userManager, IEmployeeService employeeService)
    {
        _context = context;
        _userManager = userManager;
        _employeeService = employeeService;
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
        string generatedPassword = GenerateSecurePassword();

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
                    Role = userDto.Role ?? RestaurantRole.Employee,
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
    private string GenerateSecurePassword(int length = 16)
    {
        const string lowercase = "abcdefghijklmnopqrstuvwxyz";
        const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string digits = "0123456789";
        const string special = "!@#$%^&*";
        const string allChars = lowercase + uppercase + digits + special;
    
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        var bytes = new byte[length];
        rng.GetBytes(bytes);
    
        var password = new char[length];
        password[0] = uppercase[bytes[0] % uppercase.Length];
        password[1] = lowercase[bytes[1] % lowercase.Length];
        password[2] = digits[bytes[2] % digits.Length];
        password[3] = special[bytes[3] % special.Length];
    
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[bytes[i] % allChars.Length];
        }
    
        return new string(password.OrderBy(x => Guid.NewGuid()).ToArray());
    }
}