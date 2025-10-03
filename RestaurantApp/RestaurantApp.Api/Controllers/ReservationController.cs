using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
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
    public async Task<ActionResult<ReservationBase>> GetReservation(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            
            if (reservation == null)
            {
                return NotFound($"Reservation with ID {id} not found.");
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservation {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the reservation.");
        }
    }

    // GET: api/reservation/restaurant/{restaurantId}
    [HttpGet("restaurant/{restaurantId}")]
    public async Task<ActionResult<IEnumerable<ReservationBase>>> GetReservationsByRestaurant(int userId)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsByRestaurantIdAsync(userId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations for restaurant {restaurantId}", userId);
            return StatusCode(500, "An error occurred while retrieving reservations.");
        }
    }
    
    // GET: api/reservation/client/{clientId}
    [Authorize]
    [HttpGet("client/{userId}")]
    public async Task<ActionResult<IEnumerable<ReservationBase>>> GetReservationsByUserId(string userId)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsByUserIdAsync(userId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations for restaurant {RestaurantId}", userId);
            return StatusCode(500, "An error occurred while retrieving reservations.");
        }
    }

    // POST: api/reservation
    [HttpPost]
    public async Task<ActionResult<ReservationBase>> CreateReservation([FromBody] ReservationDto reservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservation = await _reservationService.CreateReservationAsync(reservationDto);
            
            return CreatedAtAction(
                nameof(GetReservation), 
                new { id = reservation.Value.Id }, 
                reservation
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating reservation");
            return StatusCode(500, "An error occurred while creating the reservation.");
        }
    }

    // PUT: api/reservation/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReservation(int id, [FromBody] ReservationDto reservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _reservationService.UpdateReservationAsync(id, reservationDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Reservation with ID {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating reservation {Id}", id);
            return StatusCode(500, "An error occurred while updating the reservation.");
        }
    }

    // DELETE: api/reservation/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReservation(int id)
    {
        try
        {
            await _reservationService.DeleteReservationAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Reservation with ID {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting reservation {Id}", id);
            return StatusCode(500, "An error occurred while deleting the reservation.");
        }
    }

    // Table Reservation Endpoints

    // GET: api/reservation/table/{id}
    [HttpGet("tableReservation/{id}")]
    public async Task<ActionResult<TableReservation>> GetTableReservation(int id)
    {
        try
        {
            var reservation = await _reservationService.GetTableReservationByIdAsync(id);
            
            if (reservation == null)
            {
                return NotFound($"Table reservation with ID {id} not found.");
            }

            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table reservation {Id}", id);
            return StatusCode(500, "An error occurred while retrieving the table reservation.");
        }
    }
    
    // GET: api/reservation/table/{tableId}
    [HttpGet("reservation/table/{tableId}")]
    public async Task<ActionResult<IEnumerable<ReservationBase>>> GetTableReservationsByTableId(int tableId)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsByTableIdAsync(tableId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations for restaurant {tableId}", tableId);
            return StatusCode(500, "An error occurred while retrieving reservations.");
        }
    }
    
    // POST: api/reservation/table
    [HttpPost("table")]
    public async Task<ActionResult<TableReservation>> CreateTableReservation([FromBody] TableReservationDto tableReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var reservation = await _reservationService.CreateTableReservationAsync(tableReservationDto);
            
            return CreatedAtAction(
                nameof(GetTableReservation), 
                new { id = reservation.Id }, 
                reservation
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table reservation");
            return StatusCode(500, "An error occurred while creating the table reservation.");
        }
    }

    // PUT: api/reservation/table/{id}
    [HttpPut("table/{id}")]
    public async Task<IActionResult> UpdateTableReservation(int id, [FromBody] TableReservationDto tableReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            await _reservationService.UpdateTableReservationAsync(id, tableReservationDto);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Table reservation with ID {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table reservation {Id}", id);
            return StatusCode(500, "An error occurred while updating the table reservation.");
        }
    }

    // DELETE: api/reservation/table/{id}
    [HttpDelete("table/{id}")]
    public async Task<IActionResult> DeleteTableReservation(int id)
    {
        try
        {
            await _reservationService.DeleteTableReservationAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound($"Table reservation with ID {id} not found.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting table reservation {Id}", id);
            return StatusCode(500, "An error occurred while deleting the table reservation.");
        }
    }
    
    // GET: api/reservation/restaurant/{restaurantId}
    [HttpGet("manage/{userId}")]
    public async Task<ActionResult<IEnumerable<ReservationBase>>> GetReservationsToManage(string userId)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsToManage(userId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations to manage for user {userId}", userId);
            return StatusCode(500, "An error occurred while retrieving reservations.");
        }
    }
    
    [HttpGet("search")]
    [ProducesResponseType(typeof(IEnumerable<ReservationBase>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<ReservationBase>>> SearchReservations(
        [FromQuery] ReservationSearchParameters searchParams)
    {
        try
        {
            // Walidacja zakresu dat
            if (searchParams.ReservationDateFrom.HasValue && 
                searchParams.ReservationDateTo.HasValue &&
                searchParams.ReservationDateFrom > searchParams.ReservationDateTo)
            {
                return BadRequest(new { message = "Data początkowa nie może być późniejsza niż data końcowa." });
            }
            

            var reservations = await _reservationService.SearchReservationsAsync(
                searchParams.RestaurantId,
                searchParams.UserId,
                searchParams.Status,
                searchParams.CustomerName,
                searchParams.CustomerEmail,
                searchParams.CustomerPhone,
                searchParams.ReservationDate,
                searchParams.ReservationDateFrom,
                searchParams.ReservationDateTo,
                searchParams.Notes
            );
            

            return Ok(reservations.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Błąd podczas wyszukiwania rezerwacji");
            return StatusCode(500, new { message = "Wystąpił błąd podczas wyszukiwania rezerwacji." });
        }
    }
}