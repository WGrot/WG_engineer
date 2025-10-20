// RestaurantReviewsTab.razor.cs

using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.SearchParameters;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages.RestaurantDetails;

public partial class RestaurantReviewsTab : ComponentBase
{
    [Inject] private HttpClient Http { get; set; } = null!;
    [Parameter] public int Id { get; set; }
    [Parameter] public Restaurant? restaurant { get; set; }


    private List<ReviewDto>? reviews { get; set; }
    private List<ReviewDto> displayedReviews { get; set; } = new();

    private CreateReviewDto newReview { get; set; } = new() { Rating = 5 };

    private string errorInfo { get; set; } = string.Empty;
    private bool successMessage { get; set; }
    private bool isSubmitting { get; set; }
    private bool hasMoreReviews { get; set; }
    private string sortOption { get; set; } = "Newest";

    private Dictionary<int, int> ratingDistribution { get; set; } = new();

    private int reviewsPerPage = 5;
    private int currentPage = 1;
    

    protected override async Task OnParametersSetAsync()
    {
        await LoadReviews();
    }

    private async Task LoadReviews()
    {
        try
        {
            reviews = await Http.GetFromJsonAsync<List<ReviewDto>>($"/api/Reviews/restaurant/{Id}");

            if (reviews != null && reviews.Any())
            {
                CalculateStatistics();
                SortReviews(sortOption);
            }
        }
        catch (Exception ex)
        {
            errorInfo = $"Failed to load reviews: {ex.Message}";
            reviews = new List<ReviewDto>();
        }
    }

    private void CalculateStatistics()
    {
        if (reviews == null || !reviews.Any()) return;

        // Calculate rating distribution
        ratingDistribution = new Dictionary<int, int>();

        ratingDistribution[1] = restaurant.TotalRatings1Star;
        ratingDistribution[2] = restaurant.TotalRatings2Star;
        ratingDistribution[3] = restaurant.TotalRatings3Star;
        ratingDistribution[4] = restaurant.TotalRatings4Star;
        ratingDistribution[5] = restaurant.TotalRatings5Star;
    }

    private void SortReviews(string option)
    {
        sortOption = option;
        if (reviews == null) return;

        var sortedReviews = option switch
        {
            "Newest" => reviews.OrderByDescending(r => r.CreatedAt).ToList(),
            "Oldest" => reviews.OrderBy(r => r.CreatedAt).ToList(),
            "Highest" => reviews.OrderByDescending(r => r.Rating).ThenByDescending(r => r.CreatedAt).ToList(),
            "Lowest" => reviews.OrderBy(r => r.Rating).ThenByDescending(r => r.CreatedAt).ToList(),
            _ => reviews.OrderByDescending(r => r.CreatedAt).ToList()
        };

        reviews = sortedReviews;
        currentPage = 1;
        LoadDisplayedReviews();
    }

    private void LoadDisplayedReviews()
    {
        if (reviews == null) return;

        displayedReviews = reviews
            .Take(currentPage * reviewsPerPage)
            .ToList();

        hasMoreReviews = displayedReviews.Count < reviews.Count;
    }

    private void LoadMoreReviews()
    {
        currentPage++;
        LoadDisplayedReviews();
    }

    private async Task CreateReview()
    {
        if (isSubmitting) return;

        isSubmitting = true;
        errorInfo = string.Empty;
        successMessage = false;

        try
        {
            newReview.RestaurantId = Id;
            newReview.PhotosUrls ??= new List<string>();

            var response = await Http.PostAsJsonAsync("/api/Reviews", newReview);

            if (response.IsSuccessStatusCode)
            {
                successMessage = true;
                newReview = new CreateReviewDto
                {
                    RestaurantId = Id,
                    Rating = 5,
                    Content = string.Empty
                };
                await LoadReviews();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                errorInfo = $"Error adding review: {errorContent}";
            }
        }
        catch (Exception ex)
        {
            errorInfo = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private void SetRating(int rating)
    {
        newReview.Rating = rating;
    }
    
    
}