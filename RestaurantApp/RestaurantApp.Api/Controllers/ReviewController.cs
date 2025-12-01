using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.CustomHandlers.Authorization;

using RestaurantApp.Shared.Common;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Review;
using RestaurantApp.Shared.DTOs.SearchParameters;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReviewsController : ControllerBase
{
    private readonly RestaurantApp.Application.Interfaces.Services.IReviewService _reviewService;
    private readonly IAuthorizationService _authorizationService;

    public ReviewsController(RestaurantApp.Application.Interfaces.Services.IReviewService reviewService, IAuthorizationService authorizationService)
    {
        _reviewService = reviewService;
        _authorizationService = authorizationService;
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var review = await _reviewService.GetByIdAsync(id);
        return review.ToActionResult();
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var reviews = await _reviewService.GetAllAsync();
        return reviews.ToActionResult();
    }

    [HttpGet("restaurant/{restaurantId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByRestaurant(int restaurantId)
    {
        var reviews = await _reviewService.GetByRestaurantIdAsync(restaurantId);
        return reviews.ToActionResult();
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
    
        return result.ToActionResult();
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetByUser(string userId)
    {
        var reviews = await _reviewService.GetByUserIdAsync(userId);
        return reviews.ToActionResult();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto createReviewDto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null, 
            new SameUserRequirement(createReviewDto.UserId));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await _reviewService.CreateAsync(userId, createReviewDto)).ToActionResult();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReviewDto updateReviewDto)
    {
        var authResult = await _authorizationService.AuthorizeAsync(
            User, 
            null, 
            new SameUserRequirement(updateReviewDto.UserId));

        if (!authResult.Succeeded)
        {
            return Forbid();
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await _reviewService.UpdateAsync(userId, id, updateReviewDto)).ToActionResult();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return (await _reviewService.DeleteAsync(userId, id)).ToActionResult();
    }

    [HttpPatch("{id}/toggle-active")]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var result = await _reviewService.ToggleActiveStatusAsync(id);
        return result.ToActionResult();
    }

    [HttpPatch("{id}/verify")]
    public async Task<IActionResult> VerifyReview(int id)
    {
        var result = await _reviewService.VerifyReviewAsync(id);
        return result.ToActionResult();
    }
}