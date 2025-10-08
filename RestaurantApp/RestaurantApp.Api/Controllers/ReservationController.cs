using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(IReservationService reservationService, ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    // GET: api/reservation/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetReservation(int id)
    {
        var reservation = await _reservationService.GetReservationByIdAsync(id);
        return reservation.ToActionResult(this);
    }

    // GET: api/reservation/restaurant/{restaurantId}
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<IActionResult> GetReservationsByRestaurant(int userId)
    {
        var reservations = await _reservationService.GetReservationsByRestaurantIdAsync(userId);
        return reservations.ToActionResult(this);
    }

    // GET: api/reservation/client/{clientId}
    [Authorize]
    [HttpGet("client/")]
    public async Task<IActionResult> GetReservationsByUserId([FromQuery] ReservationSearchParameters searchParams)
    {
        var reservations = await _reservationService.GetReservationsByUserIdAsync(searchParams);
        return reservations.ToActionResult(this);
    }

    // POST: api/reservation
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReservation([FromBody] ReservationDto reservationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var reservation = await _reservationService.CreateReservationAsync(reservationDto);

        return reservation.ToActionResult(this);
    }

    // PUT: api/reservation/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reservationService.UpdateReservationAsync(id, reservationDto);
        return result.ToActionResult(this);
    }

    // DELETE: api/reservation/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        var result = await _reservationService.DeleteReservationAsync(id);
        return result.ToActionResult(this);
    }

    // Table Reservation Endpoints

    // GET: api/reservation/table/{id}
    [HttpGet("tableReservation/{id}")]
    public async Task<IActionResult> GetTableReservation(int id)
    {
        var reservation = await _reservationService.GetTableReservationByIdAsync(id);
        return reservation.ToActionResult(this);
    }

    // GET: api/reservation/table/{tableId}
    [HttpGet("reservation/table/{tableId}")]
    public async Task<IActionResult> GetTableReservationsByTableId(int tableId)
    {
        var reservations = await _reservationService.GetReservationsByTableIdAsync(tableId);
        return reservations.ToActionResult(this);
    }

    // POST: api/reservation/table
    [HttpPost("table")]
    [Authorize]
    public async Task<IActionResult> CreateTableReservation(
        [FromBody] TableReservationDto tableReservationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var reservation = await _reservationService.CreateTableReservationAsync(tableReservationDto);

        return reservation.ToActionResult(this);
    }

    // PUT: api/reservation/table/{id}
    [HttpPut("table/{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateTableReservation(int id, [FromBody] TableReservationDto tableReservationDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reservationService.UpdateTableReservationAsync(id, tableReservationDto);
        return result.ToActionResult(this);
    }

    // DELETE: api/reservation/table/{id}
    [HttpDelete("table/{id}")]
    public async Task<IActionResult> DeleteTableReservation(int id)
    {
        var result = await _reservationService.DeleteTableReservationAsync(id);
        return result.ToActionResult(this);
    }

    // GET: api/reservation/restaurant/{restaurantId}
    [HttpGet("manage/{userId}")]
    public async Task<IActionResult> GetReservationsToManage(string userId)
    {
        var reservations = await _reservationService.GetReservationsToManage(userId);
        return reservations.ToActionResult(this);
    }

    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ReservationBase>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchReservations(
        [FromQuery] ReservationSearchParameters searchParams)
    {
        var reservations = await _reservationService.SearchReservationsAsync(searchParams
        );
        
        return reservations.ToActionResult(this);
    }
    
    [HttpPut("manage/{id}/change-status")]
    [Authorize(Policy = "ManageReservations")]
    public async Task<IActionResult> ChangeReservationStatus(int id, [FromBody] ReservationStatus status)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _reservationService.UpdateReservationStatusAsync(id, status);
        return result.ToActionResult(this);
    }
}