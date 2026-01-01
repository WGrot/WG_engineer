using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ApplicationUser?> GetByIdAsync(string userId, CancellationToken ct)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
    }

    public async Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken: ct);
        return user?.FirstName;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken: ct);
    }

    public async Task<IEnumerable<ApplicationUser>> SearchAsync(string? firstName,
        string? lastName,
        string? phoneNumber,
        string? email,
        int? amount, 
        bool? asAdmin, CancellationToken ct)
    {
        var query = _context.Users.AsQueryable();
        if (asAdmin is false)
        {
            query = query.Where(u => u.CanBeSearched == true);  
        }

        if (!string.IsNullOrWhiteSpace(firstName))
        {
            query = query.Where(u => u.FirstName!.ToLower().Contains(firstName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(lastName))
        {
            query = query.Where(u => u.LastName!.ToLower().Contains(lastName.ToLower()));
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
        
        return await query.Take(takeAmount).ToListAsync(cancellationToken: ct);
    }

    public async Task UpdateAsync(ApplicationUser user, CancellationToken ct)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync(ct);
    }
    
    public async Task<IEnumerable<ApplicationUser>> GetByIdsAsync(IEnumerable<string> userIds, CancellationToken ct)
    {
        var idsList = userIds.ToList();
    
        if (idsList.Count == 0)
            return [];

        return await _context.Users
            .Where(u => idsList.Contains(u.Id))
            .ToListAsync(cancellationToken: ct);
    }
}