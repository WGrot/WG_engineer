﻿using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Blazor.Pages;

public partial class MyReviewsPage : ComponentBase
{
    
    [Inject] private HttpClient Http { get; set; } = null!;
    
    [Inject]
    public JwtAuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    
 private List<ReviewDto> reviews = new List<ReviewDto>();
    private string loggedUserId = "";
    private bool isLoading = true;
    private bool showEditModal = false;
    private ReviewDto? selectedReview = null;

    protected override async Task OnInitializedAsync()
    {
        await LoadUserReviews();
    }

    private async Task LoadUserReviews()
    {
        try
        {
            isLoading = true;

            var authState = await AuthStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;
            if (user.Identity?.IsAuthenticated == true)
            {
                loggedUserId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                reviews = await Http.GetFromJsonAsync<List<ReviewDto>>($"api/reviews/user/{loggedUserId}") ?? new List<ReviewDto>();
            }

            
        }
        catch (Exception ex)
        {
            // Handle error - you might want to show a toast notification here
            Console.WriteLine($"Error loading reviews: {ex.Message}");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void OpenEditModal(ReviewDto review)
    {
        selectedReview = review;
        showEditModal = true;
    }

    private void CloseEditModal()
    {
        selectedReview = null;
        showEditModal = false;
    }

    private async Task UpdateReview(ReviewDto updatedReview)
    {
        try
        {
            UpdateReviewDto dto = new UpdateReviewDto
            {
                Rating = updatedReview!.Rating,
                Content = updatedReview.Content,
                PhotosUrls = updatedReview.PhotosUrls
            };
            var response = await Http.PutAsJsonAsync($"/api/Reviews/{updatedReview!.Id}", dto);

            // Update the local list
            var index = reviews.FindIndex(r => r.Id == updatedReview.Id);
            if (index != -1)
            {
                reviews[index] = updatedReview;
            }
            
            StateHasChanged();
        }
        catch (Exception ex)
        {

        }
    }

    private async Task DeleteMyReview(int reviewId)
    {
        await DeleteReviewCore(reviewId);
    }

    private async Task DeleteReviewFromModal(int reviewId)
    {
        await DeleteReviewCore(reviewId);
    }

    private async Task DeleteReviewCore(int reviewId)
    {
        try
        {
            // Call your service to delete the review
            var response =await Http.DeleteAsync($"/api/Reviews/{reviewId}");

            // Remove from local list
            reviews.RemoveAll(r => r.Id == reviewId);
            
            StateHasChanged();
        }
        catch (Exception ex)
        {

        }
    }
}