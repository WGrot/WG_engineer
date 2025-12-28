// PageObjects/Modals/NewReservationModal.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class NewReservationModal
{
    private readonly IPage _page;

    public NewReservationModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal.show:has(.modal-title:has-text('New reservation'))");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    private ILocator LoadingContainer => Modal.Locator(".loading-container");
    
    // Date navigation
    private ILocator PreviousDayButton => Modal.Locator("button.arrow-btn:has(.bi-chevron-left)");
    private ILocator NextDayButton => Modal.Locator("button.arrow-btn:has(.bi-chevron-right)");
    private ILocator DateInput => Modal.Locator("input[type='date']");
    private ILocator DayNameText => Modal.Locator(".d-flex.flex-column strong");
    
    // Time and guests
    private ILocator StartTimeInput => Modal.Locator("#startTime");
    private ILocator EndTimeInput => Modal.Locator("#endTime");
    private ILocator GuestsInput => Modal.Locator("#guests");
    
    // Check availability
    private ILocator CheckAvailabilityButton => Modal.Locator("button:has-text('Check Available Tables')");
    
    // Available tables
    private ILocator AvailableTablesSection => Modal.Locator(".available-tables");
    private ILocator NoTablesAlert => Modal.Locator(".alert-info:has-text('No tables available')");
    private ILocator TableCards => Modal.Locator(".available-tables .col-12");
    
    // Customer info form
    private ILocator CustomerNameInput => Modal.Locator("#name");
    private ILocator CustomerPhoneInput => Modal.Locator("#phone");
    private ILocator CustomerEmailInput => Modal.Locator("#email");
    private ILocator SpecialRequestsInput => Modal.Locator("#notes");
    
    // Submit
    private ILocator ConfirmReservationButton => Modal.Locator("button:has-text('Confirm Reservation')");
    private ILocator Spinner => Modal.Locator("span:has-text('Processing...'), span:has-text('Checking...')");

    public async Task WaitForVisibleAsync()
    {
        await Modal.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task WaitForLoadAsync()
    {
        try
        {
            await LoadingContainer.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
        }
        catch (TimeoutException) { }
        
        await LoadingContainer.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    public async Task<bool> IsVisibleAsync()
    {
        return await Modal.IsVisibleAsync();
    }

    public async Task CloseAsync()
    {
        await CloseButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    // Date navigation
    public async Task GoToPreviousDayAsync()
    {
        await PreviousDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task GoToNextDayAsync()
    {
        await NextDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task SelectDateAsync(DateTime date)
    {
        await DateInput.FillAsync(date.ToString("yyyy-MM-dd"));
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<string> GetSelectedDateAsync()
    {
        return await DateInput.InputValueAsync();
    }

    // Time and guests
    public async Task SetStartTimeAsync(string time)
    {
        await StartTimeInput.FillAsync(time);
    }

    public async Task SetEndTimeAsync(string time)
    {
        await EndTimeInput.FillAsync(time);
    }

    public async Task SetGuestsAsync(int guests)
    {
        await GuestsInput.FillAsync(guests.ToString());
    }

    // Check availability
    public async Task CheckAvailabilityAsync()
    {
        await CheckAvailabilityButton.ClickAsync();
        // Wait for checking to complete
        await _page.WaitForTimeoutAsync(500);
        try
        {
            await _page.WaitForSelectorAsync(".available-tables, .alert-info:has-text('No tables')", 
                new() { Timeout = 10000 });
        }
        catch (TimeoutException) { }
    }

    public async Task<bool> HasAvailableTablesAsync()
    {
        return await TableCards.CountAsync() > 0;
    }

    public async Task<bool> HasNoTablesMessageAsync()
    {
        return await NoTablesAlert.IsVisibleAsync();
    }

    public async Task<int> GetAvailableTableCountAsync()
    {
        return await TableCards.CountAsync();
    }

    // Select table
    public async Task SelectTableByIndexAsync(int index)
    {
        await TableCards.Nth(index).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task SelectTableByNumberAsync(int tableNumber)
    {
        var tableCard = Modal.Locator($".available-tables .col-12:has-text('Table {tableNumber}')");
        await tableCard.ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    // Customer info
    public async Task FillCustomerInfoAsync(CustomerInfoData data)
    {
        if (!string.IsNullOrEmpty(data.Name))
            await CustomerNameInput.FillAsync(data.Name);
        
        if (!string.IsNullOrEmpty(data.Phone))
            await CustomerPhoneInput.FillAsync(data.Phone);
        
        if (!string.IsNullOrEmpty(data.Email))
            await CustomerEmailInput.FillAsync(data.Email);
        
        if (!string.IsNullOrEmpty(data.SpecialRequests))
            await SpecialRequestsInput.FillAsync(data.SpecialRequests);
    }

    public async Task<bool> IsCustomerInfoFormVisibleAsync()
    {
        return await CustomerNameInput.IsVisibleAsync();
    }

    // Submit reservation
    public async Task ConfirmReservationAsync()
    {
        await ConfirmReservationButton.ClickAsync();
    }

    public async Task ConfirmReservationAndWaitAsync()
    {
        await _page.RunAndWaitForResponseAsync(
            async () => await ConfirmReservationButton.ClickAsync(),
            r => r.Url.Contains("api/Reservation") && r.Request.Method == "POST",
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<bool> IsConfirmButtonVisibleAsync()
    {
        return await ConfirmReservationButton.IsVisibleAsync();
    }

    public async Task<bool> IsProcessingAsync()
    {
        return await Spinner.IsVisibleAsync();
    }

    // Full flow helper
    public async Task CreateReservationAsync(NewReservationFormData data)
    {
        // Set date/time/guests
        if (data.Date.HasValue)
            await SelectDateAsync(data.Date.Value);
        
        if (!string.IsNullOrEmpty(data.StartTime))
            await SetStartTimeAsync(data.StartTime);
        
        if (!string.IsNullOrEmpty(data.EndTime))
            await SetEndTimeAsync(data.EndTime);
        
        if (data.Guests.HasValue)
            await SetGuestsAsync(data.Guests.Value);
        
        // Check availability
        await CheckAvailabilityAsync();
        
        // Select table
        if (data.TableIndex.HasValue)
            await SelectTableByIndexAsync(data.TableIndex.Value);
        else if (data.TableNumber.HasValue)
            await SelectTableByNumberAsync(data.TableNumber.Value);
        
        // Fill customer info
        await FillCustomerInfoAsync(new CustomerInfoData
        {
            Name = data.CustomerName,
            Email = data.CustomerEmail,
            Phone = data.CustomerPhone,
            SpecialRequests = data.SpecialRequests
        });
        
        // Confirm
        await ConfirmReservationAndWaitAsync();
    }
}

public record CustomerInfoData
{
    public string? Name { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? SpecialRequests { get; init; }
}

public record NewReservationFormData
{
    public DateTime? Date { get; init; }
    public string? StartTime { get; init; }
    public string? EndTime { get; init; }
    public int? Guests { get; init; }
    public int? TableIndex { get; init; }
    public int? TableNumber { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerEmail { get; init; }
    public string? CustomerPhone { get; init; }
    public string? SpecialRequests { get; init; }
}