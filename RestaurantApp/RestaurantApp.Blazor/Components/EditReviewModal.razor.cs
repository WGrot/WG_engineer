using Microsoft.AspNetCore.Components;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.DTOs.Review;

namespace RestaurantApp.Blazor.Components;

public partial class EditReviewModal : ComponentBase
{
        [Parameter] public bool IsVisible { get; set; }
    [Parameter] public EventCallback<bool> IsVisibleChanged { get; set; }
    [Parameter] public ReviewDto? Review { get; set; }
    [Parameter] public EventCallback<ReviewDto> OnUpdate { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }
    [Parameter] public EventCallback OnClose { get; set; }

    private int tempRating;
    private string tempContent = "";
    private bool isSubmitting = false;
    private bool isDeleting = false;
    private string errorMessage = "";

    protected override void OnParametersSet()
    {
        if (Review != null && IsVisible)
        {
            tempRating = Review.Rating;
            tempContent = Review.Content ?? "";
            errorMessage = "";
        }
    }

    private void SetRating(int rating)
    {
        tempRating = rating;
    }

    private async Task HandleUpdate()
    {
        if (tempRating == 0 || string.IsNullOrWhiteSpace(tempContent))
        {
            errorMessage = "Please provide both a rating and a review.";
            return;
        }

        isSubmitting = true;
        errorMessage = "";

        try
        {
            if (Review != null)
            {
                var updatedReview = new ReviewDto
                {
                    Id = Review.Id,
                    RestaurantId = Review.RestaurantId,
                    RestaurantName = Review.RestaurantName,
                    UserId = Review.UserId,
                    UserName = Review.UserName,
                    Rating = tempRating,
                    Content = tempContent.Trim(),
                    CreatedAt = Review.CreatedAt,
                    UpdatedAt = DateTime.Now,
                    IsVerified = Review.IsVerified
                };

                await OnUpdate.InvokeAsync(updatedReview);
                await Close();
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to update review: {ex.Message}";
        }
        finally
        {
            isSubmitting = false;
        }
    }

    private async Task HandleDelete()
    {
        if (Review == null) return;

        isDeleting = true;
        errorMessage = "";

        try
        {
            await OnDelete.InvokeAsync(Review.Id);
            await Close();
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to delete review: {ex.Message}";
        }
        finally
        {
            isDeleting = false;
        }
    }

    private async Task HandleCancel()
    {
        await Close();
    }

    private async Task Close()
    {
        tempRating = 0;
        tempContent = "";
        errorMessage = "";
        isSubmitting = false;
        isDeleting = false;
        
        await IsVisibleChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
    }
}