using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly ApiDbContext _context;

    public RestaurantController(ApiDbContext context)
    {
        _context = context;
    }

    // GET: api/Restaurant
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetAll()
    {
        var restaurants = await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .ToListAsync();
            
        return Ok(restaurants);
    }

    // GET: api/Restaurant/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Restaurant>> GetById(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .ThenInclude(m => m.Items)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        return Ok(restaurant);
    }

    // GET: api/Restaurant/search?name=pizza
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Restaurant>>> Search([FromQuery] string? name = null, [FromQuery] string? address = null)
    {
        var query = _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(r => r.Name.ToLower().Contains(name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(address))
        {
            query = query.Where(r => r.Address.ToLower().Contains(address.ToLower()));
        }

        var restaurants = await query.ToListAsync();
        return Ok(restaurants);
    }

    // GET: api/Restaurant/5/tables
    [HttpGet("{id}/tables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Table>>> GetRestaurantTables(int id)
    {
        var restaurantExists = await _context.Restaurants.AnyAsync(r => r.Id == id);
        if (!restaurantExists)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        var tables = await _context.Tables
            .Where(t => t.RestaurantId == id)
            .Include(t => t.Seats)
            .ToListAsync();

        return Ok(tables);
    }

    // GET: api/Restaurant/open-now
    [HttpGet("open-now")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetOpenNow()
    {
        var now = TimeOnly.FromDateTime(DateTime.Now);
        var currentDay = DateTime.Now.DayOfWeek;

        var restaurants = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .Include(r => r.Menu)
            .ToListAsync();

        var openRestaurants = restaurants.Where(r => 
            r.OpeningHours != null && 
            r.OpeningHours.Any(oh => 
                oh.DayOfWeek == currentDay && 
                oh.IsOpenAt(now)
            )
        ).ToList();

        return Ok(openRestaurants);
    }

    // GET: api/Restaurant/5/is-open
    [HttpGet("{id}/is-open")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> IsRestaurantOpen(int id, [FromQuery] string? time = null, [FromQuery] int? dayOfWeek = null)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        var checkTime = time != null ? TimeOnly.Parse(time) : TimeOnly.FromDateTime(DateTime.Now);
        var checkDay = dayOfWeek != null ? (DayOfWeek)dayOfWeek : DateTime.Now.DayOfWeek;

        var openingHours = restaurant.OpeningHours?.FirstOrDefault(oh => oh.DayOfWeek == checkDay);
            
        if (openingHours == null)
        {
            return Ok(new { isOpen = false, message = "No opening hours defined for this day" });
        }

        var isOpen = openingHours.IsOpenAt(checkTime);
            
        return Ok(new 
        { 
            isOpen = isOpen,
            dayOfWeek = checkDay.ToString(),
            checkedTime = checkTime.ToString("HH:mm"),
            openTime = openingHours.OpenTime.ToString("HH:mm"),
            closeTime = openingHours.CloseTime.ToString("HH:mm"),
            isClosed = openingHours.IsClosed
        });
    }

    // POST: api/Restaurant
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Restaurant>> Create([FromBody] RestaurantDto restaurantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if restaurant with the same name and address already exists
        var existingRestaurant = await _context.Restaurants
            .AnyAsync(r => r.Name == restaurantDto.Name && r.Address == restaurantDto.Address);

        if (existingRestaurant)
        {
            return BadRequest($"Restaurant '{restaurantDto.Name}' already exists at this address.");
        }

        var restaurant = new Restaurant
        {
            Name = restaurantDto.Name,
            Address = restaurantDto.Address
        };

        // Add opening hours if provided
        if (restaurantDto.OpeningHours != null && restaurantDto.OpeningHours.Any())
        {
            restaurant.OpeningHours = restaurantDto.OpeningHours.Select(oh => new OpeningHours
            {
                DayOfWeek = (DayOfWeek)oh.DayOfWeek,
                OpenTime = TimeOnly.Parse(oh.OpenTime),
                CloseTime = TimeOnly.Parse(oh.CloseTime),
                IsClosed = oh.IsClosed
            }).ToList();
        }

        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        // Reload with related data
        var createdRestaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .FirstAsync(r => r.Id == restaurant.Id);

        return CreatedAtAction(nameof(GetById), new { id = createdRestaurant.Id }, createdRestaurant);
    }

    // PUT: api/Restaurant/5
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] RestaurantDto updateRestaurantDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingRestaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (existingRestaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        // Check if new name/address combination conflicts with another restaurant
        if (existingRestaurant.Name != updateRestaurantDto.Name || existingRestaurant.Address != updateRestaurantDto.Address)
        {
            var duplicateExists = await _context.Restaurants
                .AnyAsync(r => r.Name == updateRestaurantDto.Name 
                               && r.Address == updateRestaurantDto.Address 
                               && r.Id != id);

            if (duplicateExists)
            {
                return BadRequest($"Another restaurant '{updateRestaurantDto.Name}' already exists at this address.");
            }
        }

        // Update basic properties
        existingRestaurant.Name = updateRestaurantDto.Name;
        existingRestaurant.Address = updateRestaurantDto.Address;

        // Update opening hours if provided
        if (updateRestaurantDto.OpeningHours != null)
        {
            // Remove existing opening hours
            if (existingRestaurant.OpeningHours != null)
            {
                _context.OpeningHours.RemoveRange(existingRestaurant.OpeningHours);
            }

            // Add new opening hours
            existingRestaurant.OpeningHours = updateRestaurantDto.OpeningHours.Select(oh => new OpeningHours
            {
                DayOfWeek = (DayOfWeek)oh.DayOfWeek,
                OpenTime = TimeOnly.Parse(oh.OpenTime),
                CloseTime = TimeOnly.Parse(oh.CloseTime),
                IsClosed = oh.IsClosed,
                RestaurantId = id
            }).ToList();
        }

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await RestaurantExists(id))
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

    // PATCH: api/Restaurant/5/address
    [HttpPatch("{id}/address")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return BadRequest("Address cannot be empty.");
        }

        var restaurant = await _context.Restaurants.FindAsync(id);
        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        restaurant.Address = address;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // PATCH: api/Restaurant/5/opening-hours
    [HttpPatch("{id}/opening-hours")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOpeningHours(int id, [FromBody] List<OpeningHoursDto> openingHours)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        // Remove existing opening hours
        if (restaurant.OpeningHours != null)
        {
            _context.OpeningHours.RemoveRange(restaurant.OpeningHours);
        }

        // Add new opening hours
        restaurant.OpeningHours = openingHours.Select(oh => new OpeningHours
        {
            DayOfWeek = (DayOfWeek)oh.DayOfWeek,
            OpenTime = TimeOnly.Parse(oh.OpenTime),
            CloseTime = TimeOnly.Parse(oh.CloseTime),
            IsClosed = oh.IsClosed,
            RestaurantId = id
        }).ToList();

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/Restaurant/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        var restaurant = await _context.Restaurants
            .Include(r => r.Menu)
            .Include(r => r.OpeningHours)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        // Check if restaurant has any tables
        var hasTables = await _context.Tables.AnyAsync(t => t.RestaurantId == id);
        if (hasTables)
        {
            return Conflict($"Cannot delete restaurant. It has associated tables. Please delete all tables first.");
        }

        // Check if restaurant has active menu items
        if (restaurant.Menu != null)
        {
            var hasActiveMenu = await _context.Menus
                .Where(m => m.RestaurantId == id && m.IsActive)
                .AnyAsync();

            if (hasActiveMenu)
            {
                return Conflict($"Cannot delete restaurant. It has an active menu. Please deactivate the menu first.");
            }
        }

        _context.Restaurants.Remove(restaurant);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Helper method to check if restaurant exists
    private async Task<bool> RestaurantExists(int id)
    {
        return await _context.Restaurants.AnyAsync(r => r.Id == id);
    }
}