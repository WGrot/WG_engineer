using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Settings;


namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class RestaurantSettingsTab
{
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? restaurant { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private AuthService AuthService { get; set; } = default!;
    [Inject] private MessageService MessageService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private SettingsDto settings = new();
    private SettingsDto originalSettings = new();
    private bool isLoading = false;
    private bool showDeleteModal = false;
    private bool isDeleting = false;
    private bool isSaving = false;
    private bool hasChanges = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        isLoading = true;

        try
        {
            var response = await Http.GetAsync($"api/RestaurantSettings/{Id}/get-restaurant-settings");
            if (response.IsSuccessStatusCode)
            {
                settings = await response.Content.ReadFromJsonAsync<SettingsDto>() ?? new SettingsDto { RestaurantId = Id };
                originalSettings = CloneSettings(settings);
            }
            else
            {
                settings = new SettingsDto { RestaurantId = Id };
                originalSettings = CloneSettings(settings);
                await Http.PostAsJsonAsync($"api/RestaurantSettings", settings);
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "Error loading settings");
            settings = new SettingsDto { RestaurantId = Id };
            originalSettings = CloneSettings(settings);
        }
        finally
        {
            isLoading = false;
        }
    }

    private SettingsDto CloneSettings(SettingsDto source)
    {
        return new SettingsDto
        {
            Id = source.Id,
            RestaurantId = source.RestaurantId,
            ReservationsNeedConfirmation = source.ReservationsNeedConfirmation,
            MinReservationDuration = source.MinReservationDuration,
            MaxReservationDuration = source.MaxReservationDuration,
            MinAdvanceBookingTime = source.MinAdvanceBookingTime,
            MaxAdvanceBookingTime = source.MaxAdvanceBookingTime,
            MinGuestsPerReservation = source.MinGuestsPerReservation,
            MaxGuestsPerReservation = source.MaxGuestsPerReservation,
            ReservationsPerUserLimit = source.ReservationsPerUserLimit
        };
    }

    private void MarkAsChanged()
    {
        hasChanges = !AreSettingsEqual(settings, originalSettings);
        StateHasChanged();
    }

    private bool AreSettingsEqual(SettingsDto a, SettingsDto b)
    {
        return a.ReservationsNeedConfirmation == b.ReservationsNeedConfirmation &&
               a.MinReservationDuration == b.MinReservationDuration &&
               a.MaxReservationDuration == b.MaxReservationDuration &&
               a.MinAdvanceBookingTime == b.MinAdvanceBookingTime &&
               a.MaxAdvanceBookingTime == b.MaxAdvanceBookingTime &&
               a.MinGuestsPerReservation == b.MinGuestsPerReservation &&
               a.MaxGuestsPerReservation == b.MaxGuestsPerReservation &&
               a.ReservationsPerUserLimit == b.ReservationsPerUserLimit;
    }

    // Event handlers for duration settings
    private void OnMinDurationChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.MinReservationDuration = TimeSpan.FromMinutes(value);
            MarkAsChanged();
        }
    }

    private void OnMaxDurationChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.MaxReservationDuration = TimeSpan.FromMinutes(value);
            MarkAsChanged();
        }
    }
    
    private void OnMinAdvanceChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.MinAdvanceBookingTime = TimeSpan.FromHours(value);
            MarkAsChanged();
        }
    }

    private void OnMaxAdvanceChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.MaxAdvanceBookingTime = TimeSpan.FromDays(value);
            MarkAsChanged();
        }
    }
    
    private void OnMinGuestsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.MinGuestsPerReservation = value;
            MarkAsChanged();
        }
    }

    private void OnMaxGuestsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.MaxGuestsPerReservation = value;
            MarkAsChanged();
        }
    }

    private void OnReservationsPerUserChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            settings.ReservationsPerUserLimit = value;
            MarkAsChanged();
        }
    }

    private async Task ResetChanges()
    {
        settings = CloneSettings(originalSettings);
        hasChanges = false;
        StateHasChanged();
    }

    private void ShowDeleteModal()
    {
        showDeleteModal = true;
    }

    private void HideDeleteModal()
    {
        showDeleteModal = false;
    }

    private async Task DeleteRestaurant()
    {
        isDeleting = true;

        try
        {
            var response = await Http.DeleteAsync($"api/Restaurant/{Id}");
            if (response.IsSuccessStatusCode)
            {
                StateHasChanged();
                MessageService.AddInfo("Info", "Restaurant has been deleted. You will be logged out.");
                await AuthService.LogoutAsync();
                Nav.NavigateTo($"/login");
            }
            else
            {
                MessageService.AddError("Error", "Failed to delete restaurant.");
                showDeleteModal = false;
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "Failed to delete restaurant.");
            showDeleteModal = false;
        }
        finally
        {
            isDeleting = false;
            StateHasChanged();
        }
    }

    private async Task SaveChanges()
    {
        isSaving = true;

        try
        {
            var dto = new UpdateRestaurantSettingsDto
            {
                RestaurantId = settings.RestaurantId,
                ReservationsNeedConfirmation = settings.ReservationsNeedConfirmation,
                MinReservationDuration = settings.MinReservationDuration,
                MaxReservationDuration = settings.MaxReservationDuration,
                MinAdvanceBookingTime = settings.MinAdvanceBookingTime,
                MaxAdvanceBookingTime = settings.MaxAdvanceBookingTime,
                MinGuestsPerReservation = settings.MinGuestsPerReservation,
                MaxGuestsPerReservation = settings.MaxGuestsPerReservation,
                ReservationsPerUserLimit = settings.ReservationsPerUserLimit
            };

            var response = await Http.PutAsJsonAsync($"api/RestaurantSettings/{settings.Id}", dto);
            if (response.IsSuccessStatusCode)
            {
                originalSettings = CloneSettings(settings);
                hasChanges = false;

                MessageService.AddSuccess("Success", "Settings updated successfully!");
            }
            else
            {
                MessageService.AddError("Error", "Error updating settings");
            }
        }
        catch (Exception ex)
        {
            MessageService.AddError("Error", "Error updating settings");
        }
        finally
        {
            isSaving = false;
            StateHasChanged();
        }
    }
}