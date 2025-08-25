using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Shared.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantApp.Api.Models.DTOs;

namespace RestaurantApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TableController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public TableController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Table
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Table>>> GetTables()
        {
            return await _context.Tables
                .Include(t => t.Restaurant)
                .Include(t => t.Seats)
                .ToListAsync();
        }

        // GET: api/Table/5
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Table>> GetTable(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Restaurant)
                .Include(t => t.Seats)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            return Ok(table);
        }

        // GET: api/Table/restaurant/5
        [HttpGet("restaurant/{restaurantId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Table>>> GetTablesByRestaurant(int restaurantId)
        {
            var tables = await _context.Tables
                .Where(t => t.RestaurantId == restaurantId)
                .Include(t => t.Seats)
                .ToListAsync();

            return Ok(tables);
        }

        // GET: api/Table/available
        [HttpGet("available")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<Table>>> GetAvailableTables([FromQuery] int? minCapacity = null)
        {
            var query = _context.Tables
                .Include(t => t.Restaurant)
                .Include(t => t.Seats)
                .AsQueryable();

            if (minCapacity.HasValue)
            {
                query = query.Where(t => t.Capacity >= minCapacity.Value);
            }

            return Ok(await query.ToListAsync());
        }

        // POST: api/Table
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Table>> CreateTable([FromBody] CreateTableDto createTableDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Verify that the restaurant exists
            var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == createTableDto.RestaurantId);
            if (!restaurantExists)
            {
                return BadRequest($"Restaurant with ID {createTableDto.RestaurantId} does not exist.");
            }

            // Check if table number already exists in the same restaurant
            var tableNumberExists = await _context.Tables
                .AnyAsync(t => t.TableNumber == createTableDto.TableNumber && t.RestaurantId == createTableDto.RestaurantId);
            
            if (tableNumberExists)
            {
                return BadRequest($"Table number {createTableDto.TableNumber} already exists in this restaurant.");
            }

            // Create new table from DTO
            var table = new Table
            {
                TableNumber = createTableDto.TableNumber,
                Capacity = createTableDto.Capacity,
                Location = createTableDto.Location,
                RestaurantId = createTableDto.RestaurantId,
                Seats = new List<Seat>()
            };

            // Optionally create seats if provided
            if (createTableDto.SeatCount > 0)
            {
                for (int i = 1; i <= createTableDto.SeatCount; i++)
                {
                    table.Seats.Add(new Seat
                    {
                        SeatNumber = $"{table.TableNumber}-{i}",
                        IsAvailable = true,
                        Type = "Standard"
                    });
                }
            }

            _context.Tables.Add(table);
            await _context.SaveChangesAsync();

            // Load the full table with related data to return
            var createdTable = await _context.Tables
                .Include(t => t.Restaurant)
                .Include(t => t.Seats)
                .FirstAsync(t => t.Id == table.Id);

            return CreatedAtAction(nameof(GetTable), new { id = createdTable.Id }, createdTable);
        }

        // PUT: api/Table/5
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTable(int id, [FromBody] UpdateTableDto updateTableDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var existingTable = await _context.Tables.FindAsync(id);
            if (existingTable == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            // Check if new table number conflicts with another table in the same restaurant
            if (existingTable.TableNumber != updateTableDto.TableNumber)
            {
                var tableNumberExists = await _context.Tables
                    .AnyAsync(t => t.TableNumber == updateTableDto.TableNumber 
                                && t.RestaurantId == existingTable.RestaurantId 
                                && t.Id != id);
                
                if (tableNumberExists)
                {
                    return BadRequest($"Table number {updateTableDto.TableNumber} already exists in this restaurant.");
                }
            }

            // Update only the fields that can be changed
            existingTable.TableNumber = updateTableDto.TableNumber;
            existingTable.Capacity = updateTableDto.Capacity;
            existingTable.Location = updateTableDto.Location;
            // Note: RestaurantId is not updated - tables shouldn't move between restaurants

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await TableExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // PATCH: api/Table/5/capacity
        [HttpPatch("{id}/capacity")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateTableCapacity(int id, [FromBody] int capacity)
        {
            if (capacity <= 0)
            {
                return BadRequest("Capacity must be greater than 0.");
            }

            var table = await _context.Tables.FindAsync(id);
            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            table.Capacity = capacity;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Table/5
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteTable(int id)
        {
            var table = await _context.Tables
                .Include(t => t.Seats)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (table == null)
            {
                return NotFound($"Table with ID {id} not found.");
            }

            _context.Tables.Remove(table);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to check if table exists
        private async Task<bool> TableExists(int id)
        {
            return await _context.Tables.AnyAsync(t => t.Id == id);
        }
    }
}