using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs.Users;

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
    public async Task<IActionResult> GetAll()
    {
        var result = await _notificationService.GetByUserIdAsync(UserId);
        return result.ToActionResult();
    }

    [HttpGet("unread")]
    public async Task<IActionResult> GetUnread()
    {
        var result = await _notificationService.GetUnreadByUserIdAsync(UserId);
        return result.ToActionResult();
    }

    [HttpGet("unread/count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var result = await _notificationService.GetUnreadCountAsync(UserId);
        return result.ToActionResult();
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _notificationService.GetByIdAsync(id, UserId);

        return result.ToActionResult();
    }

    [HttpPatch("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var result = await _notificationService.MarkAsReadAsync(id, UserId);
        return result.ToActionResult();
    }

    [HttpPatch("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var result = await _notificationService.MarkAllAsReadAsync(UserId);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _notificationService.DeleteAsync(id, UserId);
        return result.ToActionResult();
    }

    [HttpDelete("read")]
    public async Task<IActionResult> DeleteAllRead()
    {
        await _notificationService.DeleteAllReadAsync(UserId);
        return NoContent();
    }
    
    [HttpPost]
    public async Task<IActionResult> Create(CreateNotificationDto dto)
    {
        dto.UserId = UserId;
        var result = await _notificationService.CreateAsync(dto);
        return result.ToActionResult();
    }
}