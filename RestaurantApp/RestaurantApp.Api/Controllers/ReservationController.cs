// RestaurantApp.Api/Controllers/ReservationController.cs
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization.ResourceBased.Reservations;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Reservation;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ITableReservationService _tableReservationService;
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(
        IReservationService reservationService,
        ITableReservationService tableReservationService,
        IAuthorizationService authorizationService,
        ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _tableReservationService = tableReservationService;
        _authorizationService = authorizationService;
        _logger = logger;
    }

    // ==================== BASE RESERVATION ENDPOINTS ====================

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(int id)
    {
        var result = await _reservationService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetReservationsByRestaurant(int restaurantId)
    {
        var result = await _reservationService.GetByRestaurantIdAsync(restaurantId);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("client")]
    public async Task<IActionResult> GetUserReservations([FromQuery] ReservationSearchParameters searchParams)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User is not authenticated.");

        var result = await _reservationService.GetUserReservationsAsync(userId, searchParams);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.CreateAsync(reservationDto);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto)
    {
        if (!await AuthorizeManageReservationAsync(id, requireRestaurantAccess: true))
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.UpdateAsync(id, reservationDto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        if (!await AuthorizeManageReservationAsync(id, requireRestaurantAccess: true))
            return Forbid();

        var result = await _reservationService.DeleteAsync(id);
        return result.ToActionResult();
    }

    // ==================== TABLE RESERVATION ENDPOINTS ====================

    [HttpGet("tableReservation/{id}")]
    public async Task<IActionResult> GetTableReservation(int id)
    {
        var result = await _tableReservationService.GetByIdAsync(id);
        return result.ToActionResult();
    }

    [HttpGet("reservation/table/{tableId}")]
    public async Task<IActionResult> GetTableReservationsByTableId(int tableId)
    {
        var result = await _tableReservationService.GetByTableIdAsync(tableId);
        return result.ToActionResult();
    }

    [HttpPost("table")]
    public async Task<IActionResult> CreateTableReservation([FromBody] CreateTableReservationDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _tableReservationService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpPut("table/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTableReservation(int id, [FromBody] TableReservationDto dto)
    {
        if (!await AuthorizeManageReservationAsync(id, requireRestaurantAccess: true))
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _tableReservationService.UpdateAsync(id, dto);
        return result.ToActionResult();
    }

    [HttpDelete("table/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTableReservation(int id)
    {
        if (!await AuthorizeManageReservationAsync(id, requireRestaurantAccess: true))
            return Forbid();

        var result = await _tableReservationService.DeleteAsync(id);
        return result.ToActionResult();
    }

    // ==================== MANAGEMENT ENDPOINTS ====================

    [HttpGet("manage")]
    [Authorize]
    public async Task<IActionResult> GetReservationsToManage([FromQuery] ReservationSearchParameters searchParams)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User is not authenticated.");

        var result = await _reservationService.GetManagedReservationsAsync(userId, searchParams);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchReservations([FromQuery] ReservationSearchParameters searchParams)
    {
        var result = await _reservationService.SearchAsync(searchParams);
        return result.ToActionResult();
    }

    [HttpPut("manage/{id}/change-status")]
    [Authorize]
    public async Task<IActionResult> ChangeReservationStatus(int id, [FromBody] ReservationStatusEnumDto status)
    {
        if (!await AuthorizeManageReservationAsync(id, requireRestaurantAccess: true))
            return Forbid();

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.UpdateStatusAsync(id, status);
        return result.ToActionResult();
    }

    [HttpPatch("manage/{id}/cancel-user-reservation")]
    [Authorize]
    public async Task<IActionResult> CancelUserReservation(int id)
    {
        if (!await AuthorizeManageReservationAsync(id, requireRestaurantAccess: false))
            return Forbid();

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User is not authenticated.");

        var result = await _reservationService.CancelUserReservationAsync(userId, id);
        return result.ToActionResult();
    }

    // ==================== PRIVATE HELPERS ====================

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private async Task<bool> AuthorizeManageReservationAsync(int reservationId, bool requireRestaurantAccess)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User,
            null,
            new ManageReservationRequirement(reservationId, requireRestaurantAccess));

        return authResult.Succeeded;
    }
}