using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Reservations;

public class ManageReservationAuthorizationHandler
    : AuthorizationHandler<ManageReservationRequirement>
{
    private readonly ApiDbContext _context;

    public ManageReservationAuthorizationHandler(ApiDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ManageReservationRequirement requirement)
    {
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        try
        {
            var reservationData = await _context.Reservations
                .AsNoTracking()
                .Where(r => r.Id == requirement.ReservationId)
                .Select(r => new
                {
                    r.UserId,
                    r.RestaurantId
                })
                .FirstOrDefaultAsync();

            if (reservationData == null)
            {
                context.Fail();
                return;
            }

            if (!requirement.NeedToBeEmployee)
            {
                // ŚCIEŻKA 1: Czy jest właścicielem?
                if (reservationData.UserId == userId)
                {
                    context.Succeed(requirement);
                    return;
                }
            }


            // ŚCIEŻKA 2: Czy jest pracownikiem z uprawnieniami?
            var hasPermission = await _context.RestaurantEmployees
                .AsNoTracking()
                .Where(er =>
                    er.UserId == userId &&
                    er.RestaurantId == reservationData.RestaurantId &&
                    er.IsActive)
                .SelectMany(er => er.Permissions)
                .AnyAsync(p => p.Permission == PermissionType.ManageReservations);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                context.Fail();
            }
        }
        catch (Exception)
        {
            context.Fail();
        }
    }
}