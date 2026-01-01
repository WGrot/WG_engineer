using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
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

    public ReservationController(
        IReservationService reservationService,
        ITableReservationService tableReservationService)
    {
        _reservationService = reservationService;
        _tableReservationService = tableReservationService;
    }
    

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(int id, CancellationToken ct)
    {
        var result = await _reservationService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetReservationsByRestaurant(int restaurantId, CancellationToken ct)
    {
        var result = await _reservationService.GetByRestaurantIdAsync(restaurantId, ct);
        return result.ToActionResult();
    }

    [Authorize]
    [HttpGet("client")]
    public async Task<IActionResult> GetUserReservations([FromQuery] ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User is not authenticated.");

        var result = await _reservationService.GetUserReservationsAsync(searchParams, ct);
        return result.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.CreateAsync(reservationDto, ct);
        return result.ToActionResult();
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto, CancellationToken ct)
    {

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.UpdateAsync(id, reservationDto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReservation(int id, CancellationToken ct)
    {

        var result = await _reservationService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }
    

    [HttpGet("tableReservation/{id}")]
    public async Task<IActionResult> GetTableReservation(int id, CancellationToken ct)
    {
        var result = await _tableReservationService.GetByIdAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpGet("reservation/table/{tableId}")]
    public async Task<IActionResult> GetTableReservationsByTableId(int tableId)
    {
        var result = await _tableReservationService.GetByTableIdAsync(tableId);
        return result.ToActionResult();
    }

    [HttpPost("table")]
    public async Task<IActionResult> CreateTableReservation([FromBody] CreateTableReservationDto dto, CancellationToken ct)
    {
        var result = await _tableReservationService.CreateAsync(dto, ct);
        return result.ToActionResult();
    }

    [HttpPut("table/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTableReservation(int id, [FromBody] TableReservationDto dto, CancellationToken ct)
    {
        
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _tableReservationService.UpdateAsync(id, dto, ct);
        return result.ToActionResult();
    }

    [HttpDelete("table/{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteTableReservation(int id, CancellationToken ct)
    {

        var result = await _tableReservationService.DeleteAsync(id, ct);
        return result.ToActionResult();
    }
    

    [HttpGet("manage")]
    [Authorize]
    public async Task<IActionResult> GetReservationsToManage([FromQuery] ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User is not authenticated.");

        var result = await _reservationService.GetManagedReservationsAsync(userId, searchParams, ct);
        return result.ToActionResult();
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchReservations([FromQuery] ReservationSearchParameters searchParams, CancellationToken ct)
    {
        var result = await _reservationService.SearchAsync(searchParams, ct);
        return result.ToActionResult();
    }

    [HttpPut("manage/{id}/change-status")]
    [Authorize]
    public async Task<IActionResult> ChangeReservationStatus(int id, [FromBody] ReservationStatusEnumDto status, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _reservationService.UpdateStatusAsync(id, status, ct);
        return result.ToActionResult();
    }

    [HttpPatch("manage/{id}/cancel-user-reservation")]
    [Authorize]
    public async Task<IActionResult> CancelUserReservation(int id, CancellationToken ct)
    {

        var userId = GetCurrentUserId();
        if (userId == null)
            return Unauthorized("User is not authenticated.");

        var result = await _reservationService.CancelUserReservationAsync(userId, id, ct);
        return result.ToActionResult();
    }
    

    private string? GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}