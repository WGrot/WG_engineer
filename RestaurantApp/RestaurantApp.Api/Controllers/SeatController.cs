using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantApp.Domain.Models;
using RestaurantApp.Infrastructure.Persistence;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Tables;

namespace RestaurantApp.Api.Controllers;

[Route("api/[controller]")]
    [ApiController]
    public class SeatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SeatController(ApplicationDbContext context)
        {
            _context = context;
        }

        // // GET: api/Seat
        // [HttpGet]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // public async Task<ActionResult<IEnumerable<Seat>>> GetSeats()
        // {
        //     return await _context.Seats
        //         .Include(s => s.Table)
        //             .ThenInclude(t => t.Restaurant)
        //         .ToListAsync();
        // }
        //
        // // GET: api/Seat/5
        // [HttpGet("{id}")]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<ActionResult<Seat>> GetSeat(int id)
        // {
        //     var seat = await _context.Seats
        //         .Include(s => s.Table)
        //             .ThenInclude(t => t.Restaurant)
        //         .FirstOrDefaultAsync(s => s.Id == id);
        //
        //     if (seat == null)
        //     {
        //         return NotFound($"Seat with ID {id} not found.");
        //     }
        //
        //     return Ok(seat);
        // }
        //
        // // GET: api/Seat/table/5
        // [HttpGet("table/{tableId}")]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // public async Task<ActionResult<IEnumerable<Seat>>> GetSeatsByTable(int tableId)
        // {
        //     var seats = await _context.Seats
        //         .Where(s => s.TableId == tableId)
        //         .OrderBy(s => s.SeatNumber)
        //         .ToListAsync();
        //
        //     return Ok(seats);
        // }
        //
        // // GET: api/Seat/available
        // [HttpGet("available")]
        // [ProducesResponseType(StatusCodes.Status200OK)]
        // public async Task<ActionResult<IEnumerable<Seat>>> GetAvailableSeats(
        //     [FromQuery] int? tableId = null,
        //     [FromQuery] int? restaurantId = null,
        //     [FromQuery] string? type = null)
        // {
        //     var query = _context.Seats
        //         .Include(s => s.Table)
        //             .ThenInclude(t => t.Restaurant)
        //         .Where(s => s.IsAvailable);
        //
        //     if (tableId.HasValue)
        //     {
        //         query = query.Where(s => s.TableId == tableId.Value);
        //     }
        //
        //     if (restaurantId.HasValue)
        //     {
        //         query = query.Where(s => s.Table.RestaurantId == restaurantId.Value);
        //     }
        //
        //     if (!string.IsNullOrEmpty(type))
        //     {
        //         query = query.Where(s => s.Type == type);
        //     }
        //
        //     return Ok(await query.OrderBy(s => s.Table.TableNumber).ThenBy(s => s.SeatNumber).ToListAsync());
        // }
        //
        // // POST: api/Seat
        // [HttpPost]
        // [ProducesResponseType(StatusCodes.Status201Created)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // public async Task<ActionResult<Seat>> CreateSeat([FromBody] CreateSeatDto createSeatDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     // Verify that the table exists
        //     var table = await _context.Tables
        //         .Include(t => t.Seats)
        //         .FirstOrDefaultAsync(t => t.Id == createSeatDto.TableId);
        //
        //     if (table == null)
        //     {
        //         return BadRequest($"Table with ID {createSeatDto.TableId} does not exist.");
        //     }
        //
        //     // Check if seat number already exists for this table
        //     var seatNumberExists = table.Seats.Any(s => s.SeatNumber == createSeatDto.SeatNumber);
        //     if (seatNumberExists)
        //     {
        //         return BadRequest($"Seat number {createSeatDto.SeatNumber} already exists for table {table.TableNumber}.");
        //     }
        //
        //     // Check if adding this seat would exceed table capacity
        //     if (table.Seats.Count >= table.Capacity)
        //     {
        //         return BadRequest($"Cannot add more seats. Table {table.TableNumber} has reached its capacity of {table.Capacity} seats.");
        //     }
        //
        //     var seat = new Seat
        //     {
        //         SeatNumber = createSeatDto.SeatNumber,
        //         IsAvailable = createSeatDto.IsAvailable,
        //         Type = createSeatDto.Type ?? "Standard",
        //         TableId = createSeatDto.TableId
        //     };
        //
        //     _context.Seats.Add(seat);
        //     await _context.SaveChangesAsync();
        //
        //     // Load the seat with related data
        //     var createdSeat = await _context.Seats
        //         .Include(s => s.Table)
        //             .ThenInclude(t => t.Restaurant)
        //         .FirstAsync(s => s.Id == seat.Id);
        //
        //     return CreatedAtAction(nameof(GetSeat), new { id = createdSeat.Id }, createdSeat);
        // }
        //
        // // POST: api/Seat/bulk
        // [HttpPost("bulk")]
        // [ProducesResponseType(StatusCodes.Status201Created)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // public async Task<ActionResult<IEnumerable<Seat>>> CreateSeats([FromBody] CreateSeatsDto createSeatsDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     // Verify that the table exists
        //     var table = await _context.Tables
        //         .Include(t => t.Seats)
        //         .FirstOrDefaultAsync(t => t.Id == createSeatsDto.TableId);
        //
        //     if (table == null)
        //     {
        //         return BadRequest($"Table with ID {createSeatsDto.TableId} does not exist.");
        //     }
        //
        //     // Check if adding these seats would exceed table capacity
        //     if (table.Seats.Count + createSeatsDto.Count > table.Capacity)
        //     {
        //         return BadRequest($"Cannot add {createSeatsDto.Count} seats. Table {table.TableNumber} would exceed its capacity of {table.Capacity} seats.");
        //     }
        //
        //     var seats = new List<Seat>();
        //     var startNumber = table.Seats.Count + 1;
        //
        //     for (int i = 0; i < createSeatsDto.Count; i++)
        //     {
        //         var seatNumber = createSeatsDto.SeatNumberPrefix ?? $"{table.TableNumber}-{startNumber + i}";
        //         
        //         // Check if seat number already exists
        //         if (table.Seats.Any(s => s.SeatNumber == seatNumber))
        //         {
        //             return BadRequest($"Seat number {seatNumber} already exists for table {table.TableNumber}.");
        //         }
        //
        //         seats.Add(new Seat
        //         {
        //             SeatNumber = seatNumber,
        //             IsAvailable = true,
        //             Type = createSeatsDto.Type ?? "Standard",
        //             TableId = createSeatsDto.TableId
        //         });
        //     }
        //
        //     _context.Seats.AddRange(seats);
        //     await _context.SaveChangesAsync();
        //
        //     return CreatedAtAction(nameof(GetSeatsByTable), new { tableId = createSeatsDto.TableId }, seats);
        // }
        //
        // // PUT: api/Seat/5
        // [HttpPut("{id}")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status400BadRequest)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> UpdateSeat(int id, [FromBody] UpdateSeatDto updateSeatDto)
        // {
        //     if (!ModelState.IsValid)
        //     {
        //         return BadRequest(ModelState);
        //     }
        //
        //     var existingSeat = await _context.Seats
        //         .Include(s => s.Table)
        //         .FirstOrDefaultAsync(s => s.Id == id);
        //
        //     if (existingSeat == null)
        //     {
        //         return NotFound($"Seat with ID {id} not found.");
        //     }
        //
        //     // If changing seat number, check if new number already exists for this table
        //     if (existingSeat.SeatNumber != updateSeatDto.SeatNumber)
        //     {
        //         var seatNumberExists = await _context.Seats
        //             .AnyAsync(s => s.SeatNumber == updateSeatDto.SeatNumber 
        //                         && s.TableId == existingSeat.TableId 
        //                         && s.Id != id);
        //         
        //         if (seatNumberExists)
        //         {
        //             return BadRequest($"Seat number {updateSeatDto.SeatNumber} already exists for this table.");
        //         }
        //     }
        //
        //     // Update the seat properties
        //     existingSeat.SeatNumber = updateSeatDto.SeatNumber;
        //     existingSeat.IsAvailable = updateSeatDto.IsAvailable;
        //     existingSeat.Type = updateSeatDto.Type;
        //
        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!await SeatExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }
        //
        //     return NoContent();
        // }
        //
        // // PATCH: api/Seat/5/availability
        // [HttpPatch("{id}/availability")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> UpdateSeatAvailability(int id, [FromBody] bool isAvailable)
        // {
        //     var seat = await _context.Seats.FindAsync(id);
        //     if (seat == null)
        //     {
        //         return NotFound($"Seat with ID {id} not found.");
        //     }
        //
        //     seat.IsAvailable = isAvailable;
        //     await _context.SaveChangesAsync();
        //
        //     return NoContent();
        // }
        //
        // // PATCH: api/Seat/table/5/availability
        // [HttpPatch("table/{tableId}/availability")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> UpdateTableSeatsAvailability(int tableId, [FromBody] bool isAvailable)
        // {
        //     var seats = await _context.Seats
        //         .Where(s => s.TableId == tableId)
        //         .ToListAsync();
        //
        //     if (!seats.Any())
        //     {
        //         return NotFound($"No seats found for table with ID {tableId}.");
        //     }
        //
        //     foreach (var seat in seats)
        //     {
        //         seat.IsAvailable = isAvailable;
        //     }
        //
        //     await _context.SaveChangesAsync();
        //
        //     return NoContent();
        // }
        //
        // // DELETE: api/Seat/5
        // [HttpDelete("{id}")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> DeleteSeat(int id)
        // {
        //     var seat = await _context.Seats.FindAsync(id);
        //     if (seat == null)
        //     {
        //         return NotFound($"Seat with ID {id} not found.");
        //     }
        //
        //     _context.Seats.Remove(seat);
        //     await _context.SaveChangesAsync();
        //
        //     return NoContent();
        // }
        //
        // // DELETE: api/Seat/table/5
        // [HttpDelete("table/{tableId}")]
        // [ProducesResponseType(StatusCodes.Status204NoContent)]
        // [ProducesResponseType(StatusCodes.Status404NotFound)]
        // public async Task<IActionResult> DeleteTableSeats(int tableId)
        // {
        //     var seats = await _context.Seats
        //         .Where(s => s.TableId == tableId)
        //         .ToListAsync();
        //
        //     if (!seats.Any())
        //     {
        //         return NotFound($"No seats found for table with ID {tableId}.");
        //     }
        //
        //     _context.Seats.RemoveRange(seats);
        //     await _context.SaveChangesAsync();
        //
        //     return NoContent();
        // }
        //
        // // Helper method to check if seat exists
        // private async Task<bool> SeatExists(int id)
        // {
        //     return await _context.Seats.AnyAsync(s => s.Id == id);
        // }
    }