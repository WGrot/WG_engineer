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

    private List<ReviewDto> displayedReviews { get; set; } = new();
    private CreateReviewDto newReview { get; set; } = new() { Rating = 5 };

    private string errorInfo { get; set; } = string.Empty;
    private bool successMessage { get; set; }
    private bool isSubmitting { get; set; }
    private bool isLoadingMore { get; set; }
    private bool isInitialLoading { get; set; } = true;
    private bool hasMoreReviews { get; set; }

    private string sortOption { get; set; } = "newest";
    private Dictionary<int, int> ratingDistribution { get; set; } = new();

    private int currentPage = 1;
    private int pageSize = 5;
    private int totalReviewCount = 0;
    private double averageRating = 0;

    protected override async Task OnParametersSetAsync()
    {
        await LoadInitialReviews();
    }

    private async Task LoadInitialReviews()
    {
        isInitialLoading = true;
        displayedReviews.Clear();
        currentPage = 1;

        try
        {
            await LoadReviewsPage();
        }
        catch (Exception ex)
        {
            errorInfo = $"Failed to load reviews: {ex.Message}";
        }
        finally
        {
            isInitialLoading = false;
        }
    }

    private async Task LoadReviewsPage()
    {
        try
        {
            var response = await Http.GetFromJsonAsync<PaginatedReviewsDto>(
                $"/api/Reviews/restaurant/{Id}/paginated?page={currentPage}&pageSize={pageSize}&sortBy={sortOption}");

            if (response != null)
            {
                // Dodaj nowe recenzje do wyświetlanych
                displayedReviews.AddRange(response.Reviews);
                hasMoreReviews = response.HasMore;

                totalReviewCount = restaurant.TotalReviews;
                averageRating = restaurant.AverageRating;
                SetRatingDistributionFromRestaurant();
            }
        }
        catch (Exception ex)
        {
            errorInfo = $"Failed to load reviews: {ex.Message}";
        }
    }

    private void SetRatingDistributionFromRestaurant()
    {
        if (restaurant == null) return;

        ratingDistribution = new Dictionary<int, int>
        {
            { 1, restaurant.TotalRatings1Star },
            { 2, restaurant.TotalRatings2Star },
            { 3, restaurant.TotalRatings3Star },
            { 4, restaurant.TotalRatings4Star },
            { 5, restaurant.TotalRatings5Star }
        };
    }

    private async Task LoadMoreReviews()
    {
        if (isLoadingMore || !hasMoreReviews) return;

        isLoadingMore = true;
        currentPage++;

        try
        {
            await LoadReviewsPage();
        }
        catch (Exception ex)
        {
            errorInfo = $"Failed to load more reviews: {ex.Message}";
            currentPage--; // Cofnij stronę w przypadku błędu
        }
        finally
        {
            isLoadingMore = false;
        }
    }

    private async Task SortReviews(string option)
    {
        if (sortOption == option) return;

        sortOption = option.ToLower();
        await LoadInitialReviews();
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

                // Przeładuj recenzje od początku
                await LoadInitialReviews();
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