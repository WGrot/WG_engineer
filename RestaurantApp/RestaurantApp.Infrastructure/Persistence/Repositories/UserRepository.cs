using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Users;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<string?> GetUserNameByIdAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        return user?.FirstName;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<ApplicationUser>> SearchAsync(string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount, 
        bool? asAdmin)
    {
        var query = _context.Users.AsQueryable();
        if (asAdmin is false)
        {
            query = query.Where(u => u.CanBeSearched == true);  
        }

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
            query = query.Where(u => u.PhoneNumber != null && u.PhoneNumber.Contains(phoneNumber));
        }

        if (!string.IsNullOrWhiteSpace(email))
        {
            query = query.Where(u => u.Email != null && u.Email.ToLower().Contains(email.ToLower()));
        }

        var takeAmount = amount is > 0 ? amount.Value : 50;
        
        return await query.Take(takeAmount).ToListAsync();
    }

    public async Task UpdateAsync(ApplicationUser user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    
    public async Task<IEnumerable<ApplicationUser>> GetByIdsAsync(IEnumerable<string> userIds)
    {
        var idsList = userIds.ToList();
    
        if (idsList.Count == 0)
            return [];

        return await _context.Users
            .Where(u => idsList.Contains(u.Id))
            .ToListAsync();
    }
}