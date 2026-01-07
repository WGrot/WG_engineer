using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Application.Interfaces.Services;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly IReviewService _reviewService;


    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;

    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var review = await _reviewService.GetByIdAsync(id, ct);
        return review.ToActionResult();
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var reviews = await _reviewService.GetAllAsync(ct);
        return reviews.ToActionResult();
    }

    [HttpGet("restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurant(int restaurantId, CancellationToken ct)
    {
        var reviews = await _reviewService.GetByRestaurantIdAsync(restaurantId, ct);
        return reviews.ToActionResult();
    }
    
    [HttpGet("restaurant/{restaurantId}/paginated")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurantPaginated(
        int restaurantId, 
        CancellationToken ct,
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "newest")
    {
        var result = await _reviewService.GetByRestaurantIdPaginatedAsync(
            restaurantId, 
            page, 
            pageSize, 
            sortBy, ct);
    
        return result.ToActionResult();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId, CancellationToken ct)
    {
        var reviews = await _reviewService.GetByUserIdAsync(userId, ct);
        return reviews.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto createReviewDto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await _reviewService.CreateAsync(userId, createReviewDto, ct)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto updateReviewDto, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await _reviewService.UpdateAsync(userId, id, updateReviewDto, ct)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await _reviewService.DeleteAsync(userId, id, ct)).ToActionResult();
    }

    [HttpPatch("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id, CancellationToken ct)
    {
        var result = await _reviewService.ToggleActiveStatusAsync(id, ct);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/verify")]
    public async Task<IActionResult> VerifyReview(int id, CancellationToken ct)
    {
        var result = await _reviewService.VerifyReviewAsync(id, ct);
        return result.ToActionResult();
    }
}