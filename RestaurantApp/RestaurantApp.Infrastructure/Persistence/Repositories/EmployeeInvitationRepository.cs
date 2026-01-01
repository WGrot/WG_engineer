using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class EmployeeInvitationRepository : IEmployeeInvitationRepository
{
    private readonly ApplicationDbContext _context;

    public EmployeeInvitationRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<EmployeeInvitation?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await _context.EmployeeInvitations
            .Include(i => i.Restaurant)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken: ct);
    }

    public async Task<EmployeeInvitation?> GetByTokenAsync(string token, CancellationToken ct)
    {
        return await _context.EmployeeInvitations
            .Include(i => i.Restaurant)
            .Include(i => i.User)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken: ct);
    }

    public async Task<IEnumerable<EmployeeInvitation>> GetByRestaurantIdAsync(int restaurantId, CancellationToken ct)
    {
        return await _context.EmployeeInvitations
            .Include(i => i.User)
            .Where(i => i.RestaurantId == restaurantId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<EmployeeInvitation>> GetByUserIdAsync(string userId, CancellationToken ct)
    {
        return await _context.EmployeeInvitations
            .Include(i => i.Restaurant)
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<IEnumerable<EmployeeInvitation>> GetPendingByUserIdAsync(string userId, CancellationToken ct)
    {
        return await _context.EmployeeInvitations
            .Include(i => i.Restaurant)
            .Where(i => i.UserId == userId 
                        && i.Status == InvitationStatus.Pending 
                        && i.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken: ct);
    }

    public async Task<bool> ExistsPendingAsync(int restaurantId, string userId, CancellationToken ct)
    {
        return await _context.EmployeeInvitations
            .AnyAsync(i => i.RestaurantId == restaurantId 
                           && i.UserId == userId 
                           && i.Status == InvitationStatus.Pending 
                           && i.ExpiresAt > DateTime.UtcNow, cancellationToken: ct);
    }

    public async Task<EmployeeInvitation> AddAsync(EmployeeInvitation invitation, CancellationToken ct)
    {
        await _context.EmployeeInvitations.AddAsync(invitation);
        await _context.SaveChangesAsync(ct);
        return invitation;
    }

    public async Task UpdateAsync(EmployeeInvitation invitation, CancellationToken ct)
    {
        _context.EmployeeInvitations.Update(invitation);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(EmployeeInvitation invitation, CancellationToken ct)
    {
        _context.EmployeeInvitations.Remove(invitation);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteExpiredAsync(CancellationToken ct)
    {
        var expiredInvitations = await _context.EmployeeInvitations
            .Where(i => i.ExpiresAt < DateTime.UtcNow && i.Status == InvitationStatus.Pending)
            .ToListAsync(cancellationToken: ct);

        if (expiredInvitations.Any())
        {
            _context.EmployeeInvitations.RemoveRange(expiredInvitations);
            await _context.SaveChangesAsync(ct);
        }
    }
}