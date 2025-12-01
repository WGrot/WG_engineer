using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Reservations;

public class ManageReservationAuthorizationHandler 
    : AuthorizationHandler<ManageReservationRequirement>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ManageReservationAuthorizationHandler> _logger;

    public ManageReservationAuthorizationHandler(ApplicationDbContext context, ILogger<ManageReservationAuthorizationHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageReservationRequirement requirement)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            var reservation = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == requirement.ReservationId)
                .Select(r => new { r.UserId, r.RestaurantId })
                .FirstOrDefaultAsync();

            if (reservation == null)
            {
                context.Fail();
                return;
            }
            
            if (!requirement.NeedToBeEmployee && reservation.UserId == userId)
            {
                context.Succeed(requirement);
                return;
            }
            
            var hasPermission = await _context.RestaurantEmployees
                .AsNoTracking()
                .Where(e =>
                    e.UserId == userId &&
                    e.IsActive &&
                    e.RestaurantId == reservation.RestaurantId)
                .SelectMany(e => e.Permissions)
                .AnyAsync(p => p.Permission == PermissionType.ManageReservations);

            if (hasPermission)
                context.Succeed(requirement);
            else
                context.Fail();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Authorization error in ManageReservationAuthorizationHandler");
            context.Fail();
        }
    }
}