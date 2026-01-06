using System.Net.Http.Json;
using Microsoft.AspNetCore.Components;
using RestaurantApp.Blazor.Services;
using RestaurantApp.Shared.DTOs.Restaurant;
using RestaurantApp.Shared.DTOs.Settings;


namespace RestaurantApp.Blazor.Pages.RestaurantEdit;

public partial class RestaurantSettingsTab
{
    [Parameter] public int Id { get; set; }
    [Parameter] public RestaurantDto? Restaurant { get; set; }

    [Inject] private HttpClient Http { get; set; } = default!;
    [Inject] private AuthService AuthService { get; set; } = default!;
    [Inject] private MessageService MessageService { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private SettingsDto _settings = new();
    private SettingsDto _originalSettings = new();
    private bool _isLoading = false;
    private bool _showDeleteModal = false;
    private bool _isDeleting = false;
    private bool _isSaving = false;
    private bool _hasChanges = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        _isLoading = true;

        try
        {
            var response = await Http.GetAsync($"api/RestaurantSettings/{Id}/get-restaurant-settings");
            if (response.IsSuccessStatusCode)
            {
                _settings = await response.Content.ReadFromJsonAsync<SettingsDto>() ?? new SettingsDto { RestaurantId = Id };
                _originalSettings = CloneSettings(_settings);
            }
            else
            {
                _settings = new SettingsDto { RestaurantId = Id };
                _originalSettings = CloneSettings(_settings);
                await Http.PostAsJsonAsync($"api/RestaurantSettings", _settings);
            }
        }
        catch (Exception )
        {
            MessageService.AddError("Error", "Error loading settings");
            _settings = new SettingsDto { RestaurantId = Id };
            _originalSettings = CloneSettings(_settings);
        }
        finally
        {
            _isLoading = false;
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
        _hasChanges = !AreSettingsEqual(_settings, _originalSettings);
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
            _settings.MinReservationDuration = TimeSpan.FromMinutes(value);
            MarkAsChanged();
        }
    }

    private void OnMaxDurationChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            _settings.MaxReservationDuration = TimeSpan.FromMinutes(value);
            MarkAsChanged();
        }
    }
    
    private void OnMinAdvanceChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            _settings.MinAdvanceBookingTime = TimeSpan.FromHours(value);
            MarkAsChanged();
        }
    }

    private void OnMaxAdvanceChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            _settings.MaxAdvanceBookingTime = TimeSpan.FromDays(value);
            MarkAsChanged();
        }
    }
    
    private void OnMinGuestsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            _settings.MinGuestsPerReservation = value;
            MarkAsChanged();
        }
    }

    private void OnMaxGuestsChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            _settings.MaxGuestsPerReservation = value;
            MarkAsChanged();
        }
    }

    private void OnReservationsPerUserChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var value))
        {
            _settings.ReservationsPerUserLimit = value;
            MarkAsChanged();
        }
    }

    private Task ResetChanges()
    {
        _settings = CloneSettings(_originalSettings);
        _hasChanges = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void ShowDeleteModal()
    {
        _showDeleteModal = true;
    }

    private void HideDeleteModal()
    {
        _showDeleteModal = false;
    }

    private async Task DeleteRestaurant()
    {
        _isDeleting = true;

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
                _showDeleteModal = false;
            }
        }
        catch (Exception)
        {
            MessageService.AddError("Error", "Failed to delete restaurant.");
            _showDeleteModal = false;
        }
        finally
        {
            _isDeleting = false;
            StateHasChanged();
        }
    }

    private async Task SaveChanges()
    {
        _isSaving = true;

        try
        {
            var dto = new UpdateRestaurantSettingsDto
            {
                RestaurantId = _settings.RestaurantId,
                ReservationsNeedConfirmation = _settings.ReservationsNeedConfirmation,
                MinReservationDuration = _settings.MinReservationDuration,
                MaxReservationDuration = _settings.MaxReservationDuration,
                MinAdvanceBookingTime = _settings.MinAdvanceBookingTime,
                MaxAdvanceBookingTime = _settings.MaxAdvanceBookingTime,
                MinGuestsPerReservation = _settings.MinGuestsPerReservation,
                MaxGuestsPerReservation = _settings.MaxGuestsPerReservation,
                ReservationsPerUserLimit = _settings.ReservationsPerUserLimit
            };

            var response = await Http.PutAsJsonAsync($"api/RestaurantSettings/{_settings.Id}", dto);
            if (response.IsSuccessStatusCode)
            {
                _originalSettings = CloneSettings(_settings);
                _hasChanges = false;

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
            _isSaving = false;
            StateHasChanged();
        }
    }
}