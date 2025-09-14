using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api.Models.DTOs;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly IRestaurantService _restaurantService;
    private readonly ILogger<RestaurantController> _logger;

    public RestaurantController(IRestaurantService restaurantService, ILogger<RestaurantController> logger)
    {
        _restaurantService = restaurantService;
        _logger = logger;
    }

    // GET: api/Restaurant
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetAll()
    {
        var restaurants = await _restaurantService.GetAllAsync();
        return Ok(restaurants);
    }

    // GET: api/Restaurant/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Restaurant>> GetById(int id)
    {
        var restaurant = await _restaurantService.GetByIdAsync(id);
        
        if (restaurant == null)
        {
            return NotFound($"Restaurant with ID {id} not found.");
        }

        return Ok(restaurant);
    }

    // GET: api/Restaurant/search?name=pizza
    [HttpGet("search")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Restaurant>>> Search(
        [FromQuery] string? name = null, 
        [FromQuery] string? address = null)
    {
        var restaurants = await _restaurantService.SearchAsync(name, address);
        return Ok(restaurants);
    }

    // GET: api/Restaurant/5/tables
    [HttpGet("{id}/tables")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<Table>>> GetRestaurantTables(int id)
    {
        try
        {
            var tables = await _restaurantService.GetTablesAsync(id);
            return Ok(tables);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET: api/Restaurant/open-now
    [HttpGet("open-now")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Restaurant>>> GetOpenNow()
    {
        var openRestaurants = await _restaurantService.GetOpenNowAsync();
        return Ok(openRestaurants);
    }

    // GET: api/Restaurant/5/is-open
    [HttpGet("{id}/is-open")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OpenStatusDto>> IsRestaurantOpen(
        int id, 
        [FromQuery] string? time = null, 
        [FromQuery] int? dayOfWeek = null)
    {
        try
        {
            TimeOnly? checkTime = time != null ? TimeOnly.Parse(time) : null;
            DayOfWeek? checkDay = dayOfWeek != null ? (DayOfWeek)dayOfWeek : null;
            
            var status = await _restaurantService.CheckIfOpenAsync(id, checkTime, checkDay);
            return Ok(status);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (FormatException)
        {
            return BadRequest("Invalid time format. Please use HH:mm format.");
        }
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

        try
        {
            var createdRestaurant = await _restaurantService.CreateAsync(restaurantDto);
            return CreatedAtAction(nameof(GetById), new { id = createdRestaurant.Id }, createdRestaurant);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
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

        try
        {
            await _restaurantService.UpdateAsync(id, updateRestaurantDto);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // PATCH: api/Restaurant/5/address
    [HttpPatch("{id}/address")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] string address)
    {
        try
        {
            await _restaurantService.UpdateAddressAsync(id, address);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }
    
    // PATCH: api/Restaurant/5/address
    [HttpPatch("{id}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateName(int id, [FromBody] string name)
    {
        try
        {
            await _restaurantService.UpdateNameAsync(id, name);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // PATCH: api/Restaurant/5/opening-hours
    [HttpPatch("{id}/opening-hours")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateOpeningHours(int id, [FromBody] List<OpeningHoursDto> openingHours)
    {
        try
        {
            await _restaurantService.UpdateOpeningHoursAsync(id, openingHours);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // DELETE: api/Restaurant/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _restaurantService.DeleteAsync(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}