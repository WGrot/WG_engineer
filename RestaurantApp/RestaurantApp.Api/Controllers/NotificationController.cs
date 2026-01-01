using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;


namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly IUserNotificationService _notificationService;

    public NotificationsController(IUserNotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await _notificationService.GetByUserIdAsync(UserId, ct);
        return result.ToActionResult();
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread(CancellationToken ct)
    {
        var result = await _notificationService.GetUnreadByUserIdAsync(UserId, ct);
        return result.ToActionResult();
    }

    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount(CancellationToken ct)
    {
        var result = await _notificationService.GetUnreadCountAsync(UserId, ct);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var result = await _notificationService.GetByIdAsync(id, UserId, ct);

        return result.ToActionResult();
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id, CancellationToken ct)
    {
        var result = await _notificationService.MarkAsReadAsync(id, UserId, ct);
        return result.ToActionResult();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken ct)
    {
        var result = await _notificationService.MarkAllAsReadAsync(UserId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var result = await _notificationService.DeleteAsync(id, UserId, ct);
        return result.ToActionResult();
    }

    [HttpDelete("read")]
    public async Task<IActionResult> DeleteAllRead(CancellationToken ct)
    {
        await _notificationService.DeleteAllReadAsync(UserId, ct);
        return NoContent();
    }
    
}