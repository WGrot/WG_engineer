using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class SettingsTab
{
    private readonly IPage _page;

    public SettingsTab(IPage page) => _page = page;
    
    private ILocator NeedConfirmationToggle => _page.Locator("#needConfirmationCheckbox");
    
    private ILocator MinDurationInput => _page.Locator(".col-md-6:has-text('Minimum Duration') input[type='number']");
    private ILocator MaxDurationInput => _page.Locator(".col-md-6:has-text('Maximum Duration') input[type='number']");
    
    private ILocator MinAdvanceInput => _page.Locator(".col-md-6:has-text('Minimum Advance Time') input[type='number']");
    private ILocator MaxAdvanceInput => _page.Locator(".col-md-6:has-text('Maximum Advance Time') input[type='number']");
    
    private ILocator MinGuestsInput => _page.Locator(".col-md-6:has-text('Minimum Guests') input[type='number']");
    private ILocator MaxGuestsInput => _page.Locator(".col-md-6:has-text('Maximum Guests') input[type='number']");
    private ILocator ReservationsPerUserInput => _page.Locator(".col-md-6:has-text('Reservations per User') input[type='number']");


    private ILocator SaveButton => _page.Locator("button:has-text('Save Changes')");
    private ILocator CancelButton => _page.Locator(".save-changes-card button:has-text('Cancel')");
    private ILocator UnsavedChangesCard => _page.Locator(".save-changes-card");
    private ILocator DeleteButton => _page.Locator(".card.border-danger button:has-text('Delete')");
    private ILocator DeleteConfirmModal => _page.Locator(".modal:has-text('Confirm Restaurant Deletion')");
    private ILocator LoadingSpinner => _page.Locator("text=Loading settings...");
    
    private async Task FillInputAndTriggerChangeAsync(ILocator input, string value)
    {
        await input.ClearAsync();
        await input.FillAsync(value);

        await input.DispatchEventAsync("change");

        await _page.WaitForTimeoutAsync(100);
    }
    
    public async Task SetReservationConfirmationAsync(bool required)
    {
        var isChecked = await NeedConfirmationToggle.IsCheckedAsync();
        if (isChecked != required)
        {
            await NeedConfirmationToggle.ClickAsync();
            await _page.WaitForTimeoutAsync(100);
        }
    }

    public async Task SetDurationRangeAsync(int minMinutes, int maxMinutes)
    {
        await FillInputAndTriggerChangeAsync(MinDurationInput, minMinutes.ToString());
        await FillInputAndTriggerChangeAsync(MaxDurationInput, maxMinutes.ToString());
    }

    public async Task SetBookingWindowAsync(int minHours, int maxDays)
    {
        await FillInputAndTriggerChangeAsync(MinAdvanceInput, minHours.ToString());
        await FillInputAndTriggerChangeAsync(MaxAdvanceInput, maxDays.ToString());
    }

    public async Task SetGuestLimitsAsync(int min, int max, int perUser)
    {
        await FillInputAndTriggerChangeAsync(MinGuestsInput, min.ToString());
        await FillInputAndTriggerChangeAsync(MaxGuestsInput, max.ToString());
        await FillInputAndTriggerChangeAsync(ReservationsPerUserInput, perUser.ToString());
    }

    public async Task FillAllSettingsAsync(RestaurantSettingsFormData data)
    {
        await SetReservationConfirmationAsync(data.ReservationsNeedConfirmation);
        await SetDurationRangeAsync(data.MinDurationMinutes, data.MaxDurationMinutes);
        await SetBookingWindowAsync(data.MinAdvanceHours, data.MaxAdvanceDays);
        await SetGuestLimitsAsync(data.MinGuests, data.MaxGuests, data.ReservationsPerUser);
    }


    public async Task SaveAsync()
    {
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(r => 
            r.Url.Contains("RestaurantSettings", StringComparison.OrdinalIgnoreCase) && 
            r.Request.Method == "PUT" &&
            r.Status == 200);
        await _page.WaitForSelectorAsync(".save-changes-card", new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    public async Task CancelAsync()
    {
        await CancelButton.ClickAsync();
        await _page.WaitForSelectorAsync(".save-changes-card", new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }
    
    public async Task OpenDeleteModalAsync()
    {
        await DeleteButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show", new() { Timeout = 5000 });
    }

    public async Task ConfirmDeleteRestaurantAsync()
    {
        await DeleteConfirmModal.Locator("button:has-text('Yes, Delete')").ClickAsync();
    }

    public async Task CancelDeleteAsync()
    {
        await DeleteConfirmModal.Locator("button:has-text('Cancel')").ClickAsync();
    }
    
    public async Task<bool> HasUnsavedChangesAsync()
    {
        await _page.WaitForTimeoutAsync(200);
        return await UnsavedChangesCard.IsVisibleAsync();
    }

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
        try
        {
            await _page.WaitForSelectorAsync("text=Loading settings...", new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
        }
        catch (TimeoutException)
        {
        }
        
        await _page.WaitForSelectorAsync("text=Loading settings...", new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        
        await _page.WaitForSelectorAsync("#needConfirmationCheckbox", new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
    }

    public async Task<bool> GetReservationConfirmationAsync()
    {
        return await NeedConfirmationToggle.IsCheckedAsync();
    }
    
    public async Task<int> GetMinDurationAsync()
        => int.Parse(await MinDurationInput.InputValueAsync());

    public async Task<int> GetMaxDurationAsync()
        => int.Parse(await MaxDurationInput.InputValueAsync());

    public async Task<int> GetMinAdvanceHoursAsync()
        => int.Parse(await MinAdvanceInput.InputValueAsync());

    public async Task<int> GetMaxAdvanceDaysAsync()
        => int.Parse(await MaxAdvanceInput.InputValueAsync());

    public async Task<int> GetMinGuestsAsync()
        => int.Parse(await MinGuestsInput.InputValueAsync());

    public async Task<int> GetMaxGuestsAsync()
        => int.Parse(await MaxGuestsInput.InputValueAsync());

    public async Task<int> GetReservationsPerUserAsync()
        => int.Parse(await ReservationsPerUserInput.InputValueAsync());
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