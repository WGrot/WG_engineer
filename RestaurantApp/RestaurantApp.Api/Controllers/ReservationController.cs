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
    public async Task<ActionResult<IEnumerable<ReservationBase>>> GetReservationsByRestaurant(int restaurantId)
    {
        try
        {
            var reservations = await _reservationService.GetReservationsByRestaurantIdAsync(restaurantId);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting reservations for restaurant {RestaurantId}", restaurantId);
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
                new { id = reservation.Id }, 
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
    [HttpGet("table/{id}")]
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
}