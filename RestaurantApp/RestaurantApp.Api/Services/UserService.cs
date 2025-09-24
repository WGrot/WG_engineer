using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services;

public class UserService : IUserService
{
    
    private readonly ApiDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmployeeService _employeeService;
    
    public UserService(ApiDbContext context, UserManager <ApplicationUser> userManager, IEmployeeService employeeService)
    {
        _context = context;
        _userManager = userManager;
        _employeeService = employeeService;
    }

    public async Task<ResponseUserDto> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(id));
        }

        // Pobranie użytkownika z null-checkiem
        var user = await _context.Users.FindAsync(id);
    
        if (user == null)
        {
            throw new KeyNotFoundException($"User with ID '{id}' was not found");
        }

        // Bezpieczne mapowanie
        var responseUserDto = new ResponseUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        };

        return responseUserDto;
    }

    public async Task<IEnumerable<ResponseUserDto>> SearchAsync(string? firstName, string? lastName, string? phoneNumber, string email)
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
            query = query.Where(u => u.Email.Contains(email));
        }

        List<ResponseUserDto> responseUserDtos = new List<ResponseUserDto>();

        foreach (var user in await query.ToListAsync())
        {
            responseUserDtos.Add(new ResponseUserDto
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                FirstName = user.FirstName ?? string.Empty,
                LastName = user.LastName ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty
            });
        }
        
        return responseUserDtos;
    }

    public async Task<CreateUserDto> CreateAsync(CreateUserDto userDto)
    {
        // Walidacja
        if (string.IsNullOrEmpty(userDto.Email))
            throw new ArgumentException("Email jest wymagany");

        // Sprawdź czy użytkownik już istnieje
        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
        if (existingUser != null)
            throw new InvalidOperationException("Użytkownik o podanym emailu już istnieje");

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
            throw new InvalidOperationException($"Nie udało się utworzyć użytkownika: {errors}");
        }

        // Jeśli jest RestaurantId, dodaj jako pracownika
        if (userDto.RestaurantId.HasValue)
        {
        
            var employee = new RestaurantEmployee
            {
                UserId = user.Id,
                RestaurantId = userDto.RestaurantId.Value,
                Role = userDto.Role ?? RestaurantRole.Employee, // Ustaw domyślną rolę
                Permissions = new List<RestaurantPermission>() // Pusta lista lub domyślne uprawnienia
            };
        
            await _employeeService.CreateAsync(employee);
        }

        // Zwróć DTO z wygenerowanym hasłem
        return new CreateUserDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            RestaurantId = userDto.RestaurantId,
            Password = generatedPassword
        };
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