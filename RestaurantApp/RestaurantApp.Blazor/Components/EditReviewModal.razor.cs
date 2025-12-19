using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
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
    
    [Inject] private MessageService MessageService { get; set; } = null!;
    

    private int tempRating;
    private string tempContent = "";
    private bool isSubmitting = false;
    private bool isDeleting = false;


    protected override void OnParametersSet()
    {
        if (Review != null && IsVisible)
        {
            tempRating = Review.Rating;
            tempContent = Review.Content ?? "";
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
            MessageService.AddWarning("Error", "Please provide both a rating and a review.");
            return;
        }

        isSubmitting = true;

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
                    IsVerified = Review.IsVerified,
                    RestaurantAddress = Review.RestaurantAddress
                };

                await OnUpdate.InvokeAsync(updatedReview);
                await Close();
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "Failed to update review.");
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

        try
        {
            await OnDelete.InvokeAsync(Review.Id);
            MessageService.AddSuccess("Success", "Review deleted.");
            await Close();
        }
        catch (Exception ex)
        {
           MessageService.AddError("Error", "Failed to delete review.");
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
        isSubmitting = false;
        isDeleting = false;
        
        await IsVisibleChanged.InvokeAsync(false);
        await OnClose.InvokeAsync();
    }
}