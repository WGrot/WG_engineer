using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string?> GetUserNameByIdAsync(string userId, CancellationToken ct = default)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);
        return user?.FirstName;
    }
}