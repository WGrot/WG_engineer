using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Blazor.Pages.MyReviewsPage;

public partial class MyReviewsPage : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;

    [Inject] public JwtAuthenticationStateProvider AuthStateProvider { get; set; } = default!;

    private List<ReviewDto> _reviews = new List<ReviewDto>();
    private string _loggedUserId = "";
    private bool _isLoading = true;
    private bool _showEditModal;
    private ReviewDto? _selectedReview;

    private double _averageRating;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserReviews();
        CalculateStats();
    }

    private void CalculateStats()
    {
        _averageRating = _reviews.Count == 0 ? 0 : Math.Round(_reviews.Average(r => r.Rating), 2);
    }

    private async Task LoadUserReviews()
    {
        try
        {
            _isLoading = true;

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                var loggedUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (loggedUserId != null)
                    _loggedUserId = loggedUserId;
                _reviews = await Http.GetFromJsonAsync<List<ReviewDto>>($"api/reviews/user/{_loggedUserId}") ??
                           new List<ReviewDto>();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading reviews: {ex.Message}");
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OpenEditModal(ReviewDto review)
    {
        _selectedReview = review;
        _showEditModal = true;
    }

    private void CloseEditModal()
    {
        _selectedReview = null;
        _showEditModal = false;
    }

    private async Task UpdateReview(ReviewDto updatedReview)
    {
        UpdateReviewDto dto = new UpdateReviewDto
        {
            Rating = updatedReview.Rating,
            Content = updatedReview.Content,
            PhotosUrls = updatedReview.PhotosUrls,
            UserId = updatedReview.UserId,
        };
        var response = await Http.PutAsJsonAsync($"/api/Reviews/{updatedReview.Id}", dto);

        var index = _reviews.FindIndex(r => r.Id == updatedReview.Id);
        if (index != -1)
        {
            _reviews[index] = updatedReview;
        }

        StateHasChanged();
    }


    private async Task DeleteReviewFromModal(int reviewId)
    {
        await DeleteReviewCore(reviewId);
    }

    private async Task DeleteReviewCore(int reviewId)
    {
        var response = await Http.DeleteAsync($"/api/Reviews/{reviewId}");

        _reviews.RemoveAll(r => r.Id == reviewId);

        StateHasChanged();
    }
}