using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Functional;
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

    public async Task<Either<ApiError, ResponseUserDto>> GetByIdAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return Either<ApiError, ResponseUserDto>.Left(ApiError.BadRequest("User ID cannot be null or empty"));

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return Either<ApiError, ResponseUserDto>.Left(ApiError.NotFound($"User with ID '{id}' was not found"));

        return Either<ApiError, ResponseUserDto>.Right(new ResponseUserDto
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName ?? string.Empty,
            LastName = user.LastName ?? string.Empty,
            PhoneNumber = user.PhoneNumber ?? string.Empty
        });
    }

    public async Task<Either<ApiError, IEnumerable<ResponseUserDto>>> SearchAsync(string? firstName, string? lastName, string? phoneNumber, string? email)
    {
        var query = _context.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(firstName))
            query = query.Where(u => u.FirstName.ToLower().Contains(firstName.ToLower()));

        if (!string.IsNullOrWhiteSpace(lastName))
            query = query.Where(u => u.LastName.ToLower().Contains(lastName.ToLower()));

        if (!string.IsNullOrWhiteSpace(phoneNumber))
            query = query.Where(u => u.PhoneNumber.Contains(phoneNumber));

        if (!string.IsNullOrWhiteSpace(email))
            query = query.Where(u => u.Email.Contains(email));

        var users = await query.ToListAsync();

        var dtos = users.Select(u => new ResponseUserDto
        {
            Id = u.Id,
            Email = u.Email ?? string.Empty,
            FirstName = u.FirstName ?? string.Empty,
            LastName = u.LastName ?? string.Empty,
            PhoneNumber = u.PhoneNumber ?? string.Empty
        }).ToList();

        return Either<ApiError, IEnumerable<ResponseUserDto>>.Right(dtos);
    }

    public async Task<Either<ApiError, CreateUserDto>> CreateAsync(CreateUserDto userDto)
    {
        if (string.IsNullOrEmpty(userDto.Email))
            return Either<ApiError, CreateUserDto>.Left(ApiError.BadRequest("Email jest wymagany"));

        var existingUser = await _userManager.FindByEmailAsync(userDto.Email);
        if (existingUser != null)
            return Either<ApiError, CreateUserDto>.Left(ApiError.Conflict("Użytkownik o podanym emailu już istnieje"));

        string generatedPassword = GenerateSecurePassword();

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
            return Either<ApiError, CreateUserDto>.Left(ApiError.Internal($"Nie udało się utworzyć użytkownika: {errors}"));
        }

        if (userDto.RestaurantId.HasValue)
        {
            var employee = new RestaurantEmployee
            {
                UserId = user.Id,
                RestaurantId = userDto.RestaurantId.Value,
                Role = userDto.Role ?? RestaurantRole.Employee,
                Permissions = new List<RestaurantPermission>()
            };

            await _employeeService.CreateAsync(employee);
        }

        return Either<ApiError, CreateUserDto>.Right(new CreateUserDto
        {
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            RestaurantId = userDto.RestaurantId,
            Password = generatedPassword
        });
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
            password[i] = allChars[bytes[i] % allChars.Length];

        return new string(password.OrderBy(x => Guid.NewGuid()).ToArray());
    }
}