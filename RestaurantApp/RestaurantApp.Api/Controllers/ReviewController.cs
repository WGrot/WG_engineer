using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;

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
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        return review.ToActionResult(this);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _reviewService.GetAllAsync();
        return reviews.ToActionResult(this);
    }

    [HttpGet("restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
    {
        var reviews = await _reviewService.GetByRestaurantIdAsync(restaurantId);
        return reviews.ToActionResult(this);
    }
    
    [HttpGet("restaurant/{restaurantId}/paginated")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurantPaginated(
        int restaurantId, 
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 5,
        [FromQuery] string sortBy = "newest")
    {
        var result = await _reviewService.GetByRestaurantIdPaginatedAsync(
            restaurantId, 
            page, 
            pageSize, 
            sortBy);
    
        return result.ToActionResult(this);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
    {
        var reviews = await _reviewService.GetByUserIdAsync(userId);
        return reviews.ToActionResult(this);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto createReviewDto)
    {
        var review = await _reviewService.CreateAsync(createReviewDto);
        return review.ToActionResult(this);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto updateReviewDto)
    {
        var review = await _reviewService.UpdateAsync(id, updateReviewDto);
        return review.ToActionResult(this);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _reviewService.DeleteAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _reviewService.ToggleActiveStatusAsync(id);
        return result.ToActionResult(this);
    }

    [HttpPatch("{id}/verify")]
    public async Task<IActionResult> VerifyReview(int id)
    {
        var result = await _reviewService.VerifyReviewAsync(id);
        return result.ToActionResult(this);
    }
}