using Microsoft.EntityFrameworkCore;
using RestaurantApp.Application.Interfaces.Repositories;
using RestaurantApp.Domain.Models;

namespace RestaurantApp.Infrastructure.Persistence.Repositories;

public class UserNotificationRepository : IUserNotificationRepository
{
    private readonly ApplicationDbContext _context;

    public UserNotificationRepository(ApplicationDbContext context)
    {
        _context = context;
    }



    public async Task<UserNotification?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        return await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task<List<UserNotification>> GetByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.UserNotifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<UserNotification>> GetUnreadByUserIdAsync(string userId, CancellationToken ct = default)
    {
        return await _context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<int> GetUnreadCountAsync(string userId, CancellationToken ct = default)
    {
        return await _context.UserNotifications
            .CountAsync(n => n.UserId == userId && !n.IsRead, ct);
    }

    public async Task<UserNotification> AddAsync(UserNotification notification, CancellationToken ct = default)
    {
        _context.UserNotifications.Add(notification);
        await _context.SaveChangesAsync(ct);
        return notification;
    }

    public async Task AddRangeAsync(IEnumerable<UserNotification> notifications, CancellationToken ct = default)
    {
        _context.UserNotifications.AddRange(notifications);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(UserNotification notification, CancellationToken ct = default)
    {
        _context.UserNotifications.Update(notification);
        await _context.SaveChangesAsync(ct);
    }

    public async Task MarkAsReadAsync(int id, string userId, CancellationToken ct = default)
    {
        var notification = await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

        if (notification is not null)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task MarkAllAsReadAsync(string userId, CancellationToken ct = default)
    {
        var notifications = await _context.UserNotifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync(ct);

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
            notification.ReadAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(int id, string userId, CancellationToken ct = default)
    {
        var notification = await _context.UserNotifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId, ct);

        if (notification is not null)
        {
            _context.UserNotifications.Remove(notification);
            await _context.SaveChangesAsync(ct);
        }
    }

    public async Task DeleteAllReadAsync(string userId, CancellationToken ct = default)
    {
        var notifications = await _context.UserNotifications
            .Where(n => n.UserId == userId && n.IsRead)
            .ToListAsync(ct);

        _context.UserNotifications.RemoveRange(notifications);
        await _context.SaveChangesAsync(ct);
    }
}