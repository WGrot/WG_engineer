using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Services;

public class UserService : IUserService
{
    
    private readonly ApiDbContext _context;
    
    public UserService(ApiDbContext context)
    {
        _context = context;
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

}