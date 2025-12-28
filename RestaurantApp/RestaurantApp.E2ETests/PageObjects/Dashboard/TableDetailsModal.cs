// PageObjects/Modals/TableDetailsModal.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class TableDetailsModal
{
    private readonly IPage _page;

    public TableDetailsModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal.show:has(.modal-title:has-text('Table'))");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    
    // Table info
    private ILocator TableNumberText => Modal.Locator("p:has-text('Table Number:')");
    private ILocator SeatsText => Modal.Locator("p:has-text('Seats:')");
    private ILocator LocationText => Modal.Locator("p:has-text('Location:')");
    
    // Date navigation
    private ILocator PreviousDayButton => Modal.Locator("button.arrow-btn:has(.bi-chevron-left)");
    private ILocator NextDayButton => Modal.Locator("button.arrow-btn:has(.bi-chevron-right)");
    private ILocator DateInput => Modal.Locator("input[type='date']");
    private ILocator DayNameText => Modal.Locator(".d-flex.flex-column strong");
    
    // Availability map
    private ILocator AvailabilityMap => Modal.Locator("app-availability-map, [class*='availability']");
    private ILocator AvailableSlots => Modal.Locator(".segment-zero, [class*='available']");
    
    // Reservation section (TableReservationSection)
    private ILocator ReservationSection => Modal.Locator(".container.mt-4:has(h3:has-text('Book Table'))");
    private ILocator StartTimeInput => Modal.Locator("#startTime");
    private ILocator EndTimeInput => Modal.Locator("#endTime");
    private ILocator GuestsInput => Modal.Locator("#guests");
    private ILocator CustomerNameInput => Modal.Locator("#name");
    private ILocator CustomerPhoneInput => Modal.Locator("#phone");
    private ILocator CustomerEmailInput => Modal.Locator("#email");
    private ILocator SpecialRequestsInput => Modal.Locator("#notes");
    private ILocator ConfirmReservationButton => Modal.Locator("button:has-text('Confirm Reservation')");
    private ILocator ProcessingSpinner => Modal.Locator(".spinner-border");
    private ILocator TimeWarningAlert => Modal.Locator(".alert-warning:has-text('end time is after the start time')");
    private ILocator MaxCapacityText => Modal.Locator("small:has-text('Max capacity')");

    public async Task WaitForVisibleAsync()
    {
        await Modal.WaitForAsync(new() { Timeout = 5000 });
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

    public async Task<string> GetTitleAsync()
    {
        return (await Modal.Locator(".modal-title").InnerTextAsync()).Trim();
    }

    public async Task<int> GetTableNumberFromTitleAsync()
    {
        var title = await GetTitleAsync();
        var match = System.Text.RegularExpressions.Regex.Match(title, @"Table (\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    public async Task<TableDetailsData> GetTableInfoAsync()
    {
        var data = new TableDetailsData();

        var tableNumberText = await TableNumberText.InnerTextAsync();
        if (int.TryParse(tableNumberText.Replace("Table Number:", "").Trim(), out int tableNum))
        {
            data.TableNumber = tableNum;
        }

        var seatsText = await SeatsText.InnerTextAsync();
        if (int.TryParse(seatsText.Replace("Seats:", "").Trim(), out int seats))
        {
            data.Seats = seats;
        }

        var locationText = await LocationText.InnerTextAsync();
        data.Location = locationText.Replace("Location:", "").Trim();

        return data;
    }

    // Date navigation
    public async Task GoToPreviousDayAsync()
    {
        await PreviousDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task GoToNextDayAsync()
    {
        await NextDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task SelectDateAsync(DateTime date)
    {
        await DateInput.FillAsync(date.ToString("yyyy-MM-dd"));
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task<string> GetSelectedDateAsync()
    {
        return await DateInput.InputValueAsync();
    }

    public async Task<string> GetDayNameAsync()
    {
        return (await DayNameText.InnerTextAsync()).Trim();
    }

    // Availability map interaction
    public async Task ClickAvailableSlotAsync(int slotIndex)
    {
        var slots = AvailableSlots;
        var count = await slots.CountAsync();
        
        if (slotIndex >= count)
        {
            throw new InvalidOperationException($"Slot index {slotIndex} is out of range. Available slots: {count}");
        }
        
        await slots.Nth(slotIndex).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task<int> GetAvailableSlotsCountAsync()
    {
        return await AvailableSlots.CountAsync();
    }

    // Reservation section checks
    public async Task<bool> IsReservationSectionVisibleAsync()
    {
        return await ReservationSection.IsVisibleAsync();
    }

    public async Task<bool> IsConfirmButtonVisibleAsync()
    {
        return await ConfirmReservationButton.IsVisibleAsync();
    }

    public async Task<bool> IsTimeWarningVisibleAsync()
    {
        return await TimeWarningAlert.IsVisibleAsync();
    }

    public async Task<bool> IsProcessingAsync()
    {
        return await ProcessingSpinner.IsVisibleAsync();
    }

    // Reservation form - time and guests
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

    public async Task<string> GetStartTimeAsync()
    {
        return await StartTimeInput.InputValueAsync();
    }

    public async Task<string> GetEndTimeAsync()
    {
        return await EndTimeInput.InputValueAsync();
    }

    public async Task<int> GetMaxCapacityAsync()
    {
        var text = await MaxCapacityText.InnerTextAsync();
        var match = System.Text.RegularExpressions.Regex.Match(text, @"(\d+)");
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    // Reservation form - customer info
    public async Task FillCustomerInfoAsync(TableReservationCustomerData data)
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

    public async Task<bool> IsCustomerFormVisibleAsync()
    {
        return await CustomerNameInput.IsVisibleAsync();
    }

    // Confirm reservation
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

    // Full flow helper for creating reservation via table details
    public async Task CreateReservationAsync(TableReservationFormData data)
    {
        // Set time
        if (!string.IsNullOrEmpty(data.StartTime))
            await SetStartTimeAsync(data.StartTime);
        
        if (!string.IsNullOrEmpty(data.EndTime))
            await SetEndTimeAsync(data.EndTime);
        
        if (data.Guests.HasValue)
            await SetGuestsAsync(data.Guests.Value);
        
        // Fill customer info
        await FillCustomerInfoAsync(new TableReservationCustomerData
        {
            Name = data.CustomerName,
            Phone = data.CustomerPhone,
            Email = data.CustomerEmail,
            SpecialRequests = data.SpecialRequests
        });
        
        // Confirm
        await ConfirmReservationAndWaitAsync();
    }
}

public record TableDetailsData
{
    public int TableNumber { get; set; }
    public int Seats { get; set; }
    public string? Location { get; set; }
}

public record TableReservationCustomerData
{
    public string? Name { get; init; }
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string? SpecialRequests { get; init; }
}

public record TableReservationFormData
{
    public string? StartTime { get; init; }
    public string? EndTime { get; init; }
    public int? Guests { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerPhone { get; init; }
    public string? CustomerEmail { get; init; }
    public string? SpecialRequests { get; init; }
}