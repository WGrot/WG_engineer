using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class SettingsTab
{
    private readonly IPage _page;

    public SettingsTab(IPage page) => _page = page;

    // Locators - Reservation Confirmation
    private ILocator NeedConfirmationToggle => _page.Locator("#needConfirmationCheckbox");

    // Locators - Duration
    private ILocator MinDurationInput => _page.Locator(".card:has-text('Reservation Duration') input").First;
    private ILocator MaxDurationInput => _page.Locator(".card:has-text('Reservation Duration') input").Last;

    // Locators - Booking Window
    private ILocator MinAdvanceInput => _page.Locator(".card:has-text('Booking Window') input").First;
    private ILocator MaxAdvanceInput => _page.Locator(".card:has-text('Booking Window') input").Last;

    // Locators - Guest Limits
    private ILocator MinGuestsInput => _page.Locator("input").Filter(new() { Has = _page.Locator("xpath=../following-sibling::span[contains(text(),'guests')]") }).First;
    private ILocator MaxGuestsInput => _page.Locator(".col-md-6:has-text('Maximum Guests') input");
    private ILocator ReservationsPerUserInput => _page.Locator(".col-md-6:has-text('Reservations per User') input");

    // Locators - Actions
    private ILocator SaveButton => _page.Locator("button:has-text('Save Changes')");
    private ILocator CancelButton => _page.Locator(".save-changes-card button:has-text('Cancel')");
    private ILocator UnsavedChangesCard => _page.Locator(".save-changes-card");
    private ILocator DeleteButton => _page.Locator(".card.border-danger button:has-text('Delete')");
    private ILocator DeleteConfirmModal => _page.Locator(".modal:has-text('Confirm Restaurant Deletion')");
    private ILocator LoadingSpinner => _page.Locator("text=Loading settings...");

    // Actions - Settings
    public async Task SetReservationConfirmationAsync(bool required)
    {
        var isChecked = await NeedConfirmationToggle.IsCheckedAsync();
        if (isChecked != required)
            await NeedConfirmationToggle.ClickAsync();
    }

    public async Task SetDurationRangeAsync(int minMinutes, int maxMinutes)
    {
        await MinDurationInput.FillAsync(minMinutes.ToString());
        await MaxDurationInput.FillAsync(maxMinutes.ToString());
    }

    public async Task SetBookingWindowAsync(int minHours, int maxDays)
    {
        await MinAdvanceInput.FillAsync(minHours.ToString());
        await MaxAdvanceInput.FillAsync(maxDays.ToString());
    }

    public async Task SetGuestLimitsAsync(int min, int max, int perUser)
    {
        await MinGuestsInput.FillAsync(min.ToString());
        await MaxGuestsInput.FillAsync(max.ToString());
        await ReservationsPerUserInput.FillAsync(perUser.ToString());
    }

    public async Task FillAllSettingsAsync(RestaurantSettingsFormData data)
    {
        await SetReservationConfirmationAsync(data.ReservationsNeedConfirmation);
        await SetDurationRangeAsync(data.MinDurationMinutes, data.MaxDurationMinutes);
        await SetBookingWindowAsync(data.MinAdvanceHours, data.MaxAdvanceDays);
        await SetGuestLimitsAsync(data.MinGuests, data.MaxGuests, data.ReservationsPerUser);
    }

    // Actions - Save/Cancel
    public async Task SaveAsync()
    {
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(r => r.Url.Contains("settings") && r.Status == 200);
        await _page.WaitForSelectorAsync(".save-changes-card", new() { State = WaitForSelectorState.Hidden });
    }

    public async Task CancelAsync()
    {
        await CancelButton.ClickAsync();
    }

    // Actions - Delete Restaurant
    public async Task OpenDeleteModalAsync()
    {
        await DeleteButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show");
    }

    public async Task ConfirmDeleteRestaurantAsync()
    {
        await DeleteConfirmModal.Locator("button:has-text('Yes, Delete')").ClickAsync();
    }

    public async Task CancelDeleteAsync()
    {
        await DeleteConfirmModal.Locator("button:has-text('Cancel')").ClickAsync();
    }

    // State checks
    public async Task<bool> HasUnsavedChangesAsync() 
        => await UnsavedChangesCard.IsVisibleAsync();

    public async Task<bool> IsLoadingAsync() 
        => await LoadingSpinner.IsVisibleAsync();

    public async Task<bool> IsReservationConfirmationEnabledAsync() 
        => await NeedConfirmationToggle.IsCheckedAsync();

    public async Task<RestaurantSettingsFormData> GetCurrentValuesAsync()
    {
        return new RestaurantSettingsFormData
        {
            ReservationsNeedConfirmation = await NeedConfirmationToggle.IsCheckedAsync(),
            MinDurationMinutes = int.Parse(await MinDurationInput.InputValueAsync()),
            MaxDurationMinutes = int.Parse(await MaxDurationInput.InputValueAsync()),
            MinAdvanceHours = int.Parse(await MinAdvanceInput.InputValueAsync()),
            MaxAdvanceDays = int.Parse(await MaxAdvanceInput.InputValueAsync()),
            MinGuests = int.Parse(await MinGuestsInput.InputValueAsync()),
            MaxGuests = int.Parse(await MaxGuestsInput.InputValueAsync()),
            ReservationsPerUser = int.Parse(await ReservationsPerUserInput.InputValueAsync())
        };
    }

    public async Task WaitForLoadAsync()
    {
        await _page.WaitForSelectorAsync("text=Loading settings...", new() { State = WaitForSelectorState.Hidden });
    }
}

public record RestaurantSettingsFormData
{
    public bool ReservationsNeedConfirmation { get; init; }
    public int MinDurationMinutes { get; init; } = 30;
    public int MaxDurationMinutes { get; init; } = 180;
    public int MinAdvanceHours { get; init; } = 1;
    public int MaxAdvanceDays { get; init; } = 30;
    public int MinGuests { get; init; } = 1;
    public int MaxGuests { get; init; } = 10;
    public int ReservationsPerUser { get; init; } = 5;
}