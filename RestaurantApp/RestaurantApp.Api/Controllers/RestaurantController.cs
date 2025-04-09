using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Api;
using RestaurantApp.Shared;

[ApiController]
[Route("api/[controller]")]
public class RestaurantController : ControllerBase
{
    private readonly ApiDbContext _context;

    public RestaurantController(ApiDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var restaurants = await _context.Restaurants.ToListAsync();
        return Ok(restaurants);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Restaurant restaurant)
    {
        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAll), new { id = restaurant.Id }, restaurant);
    }
}