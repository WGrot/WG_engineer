using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
public class DetailsTableBookingTab
{
    private readonly IPage _page;

    public DetailsTableBookingTab(IPage page)
    {
        _page = page;
    }

    private ILocator BookingContainer => _page.Locator(".booking-container");

    private ILocator SuccessToast => _page.Locator(".toast.border-success");
    private ILocator ErrorToast => _page.Locator(".toast.border-danger");
    private ILocator SuccessToastBody => _page.Locator(".toast.border-success .toast-body");
    private ILocator ErrorToastBody => _page.Locator(".toast.border-danger .toast-body");

    private ILocator LoadingSpinner => _page.Locator(".booking-container .loading-container .spinner");
    private ILocator LoadingText => _page.Locator(".booking-container .loading-container p:has-text('Loading')");

    private ILocator ErrorContainer => _page.Locator(".booking-container .error");
    private ILocator GoBackButton => _page.Locator(".booking-container .error button:has-text('Go Back')");

    private ILocator BookingHeader => _page.Locator("h3:has-text('Check table availability')");

    private ILocator PreviousDayButton => _page.Locator(".arrow-btn:has(.bi-chevron-left)");
    private ILocator NextDayButton => _page.Locator(".arrow-btn:has(.bi-chevron-right)");
    private ILocator SelectedDayName => _page.Locator(".d-flex.flex-column.align-items-center strong");
    private ILocator DateInput => _page.Locator("input[type='date']");

    private ILocator StartTimeInput => _page.Locator("#startTime");
    private ILocator EndTimeInput => _page.Locator("#endTime");

    private ILocator GuestCountInput => _page.Locator("#guests");

    private ILocator CheckAvailabilityButton => _page.Locator("button:has-text('Check Available Tables')");
    private ILocator CheckingAvailabilityButton => _page.Locator("button:has-text('Checking...')");

    private ILocator AvailableTablesSection => _page.Locator(".available-tables");
    private ILocator AvailableTablesHeader => _page.Locator(".available-tables h3:has-text('Available Tables')");
    private ILocator NoTablesAlert => _page.Locator(".alert-info:has-text('No tables available')");
    private ILocator TableCards => _page.Locator(".available-tables .row .col-12, .available-tables .row .col-sm-6, .available-tables .row .col-md-4, .available-tables .row .col-lg-3");

    private ILocator CustomerInfoHeader => _page.Locator("h4:has-text('Customer Information')");
    private ILocator CustomerNameInput => _page.Locator("#name");
    private ILocator CustomerPhoneInput => _page.Locator("#phone");
    private ILocator CustomerEmailInput => _page.Locator("#email");
    private ILocator SpecialRequestsTextarea => _page.Locator("#notes");

    private ILocator ConfirmReservationButton => _page.Locator("button:has-text('Confirm Reservation')");
    private ILocator ProcessingButton => _page.Locator("button:has-text('Processing...')");

    #region Loading State
    
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    public async Task WaitForFormLoadAsync()
    {
        await _page.WaitForSelectorAsync(".booking-container .loading-container", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }
    
    public async Task<bool> IsRestaurantNotFoundAsync()
    {
        return await ErrorContainer.IsVisibleAsync();
    }

    public async Task ClickGoBackAsync()
    {
        await GoBackButton.ClickAsync();
    }

    #endregion

    #region Date Selection
    
    public async Task<DateTime> GetSelectedDateAsync()
    {
        var dateString = await DateInput.InputValueAsync();
        return DateTime.Parse(dateString);
    }
    
    public async Task<string> GetSelectedDayNameAsync()
    {
        return await SelectedDayName.InnerTextAsync();
    }
    
    public async Task SelectDateAsync(DateTime date)
    {
        var dateString = date.ToString("yyyy-MM-dd");
        await DateInput.FillAsync(dateString);
        await DateInput.DispatchEventAsync("change");
        await _page.WaitForTimeoutAsync(300);
    }
    
    public async Task ClickPreviousDayAsync()
    {
        await PreviousDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(200);
    }

    public async Task ClickNextDayAsync()
    {
        await NextDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(200);
    }

    #endregion

    #region Time Selection
    
    public async Task<string> GetStartTimeAsync()
    {
        return await StartTimeInput.InputValueAsync();
    }
    
    public async Task SetStartTimeAsync(TimeOnly time)
    {
        var timeString = time.ToString("HH:mm");
        await StartTimeInput.FillAsync(timeString);
    }
    
    public async Task SetStartTimeAsync(string time)
    {
        await StartTimeInput.FillAsync(time);
    }
    
    public async Task<string> GetEndTimeAsync()
    {
        return await EndTimeInput.InputValueAsync();
    }
    
    public async Task SetEndTimeAsync(TimeOnly time)
    {
        var timeString = time.ToString("HH:mm");
        await EndTimeInput.FillAsync(timeString);
    }
    
    public async Task SetEndTimeAsync(string time)
    {
        await EndTimeInput.FillAsync(time);
    }

    #endregion

    #region Guest Count
    
    public async Task<int> GetGuestCountAsync()
    {
        var value = await GuestCountInput.InputValueAsync();
        return int.Parse(value);
    }
    
    public async Task SetGuestCountAsync(int count)
    {
        await GuestCountInput.FillAsync(count.ToString());
    }

    #endregion

    #region Check Availability
    
    public async Task<bool> IsCheckAvailabilityButtonVisibleAsync()
    {
        return await CheckAvailabilityButton.IsVisibleAsync();
    }
    
    public async Task<bool> IsCheckingAvailabilityAsync()
    {
        return await CheckingAvailabilityButton.IsVisibleAsync();
    }
    
    public async Task ClickCheckAvailabilityAsync()
    {
        await CheckAvailabilityButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    public async Task WaitForAvailabilityCheckAsync()
    {
        // Wait for checking to start
        await _page.WaitForTimeoutAsync(200);
        // Wait for checking to finish
        await _page.WaitForSelectorAsync("button:has-text('Checking...')", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
    }

    #endregion

    #region Available Tables


    public async Task<bool> IsAvailableTablesSectionVisibleAsync()
    {
        return await AvailableTablesSection.IsVisibleAsync();
    }
    
    public async Task<bool> IsNoTablesAvailableAsync()
    {
        return await NoTablesAlert.IsVisibleAsync();
    }
    
    public async Task<int> GetAvailableTableCountAsync()
    {
        if (await IsNoTablesAvailableAsync())
            return 0;

        return await TableCards.CountAsync();
    }
    
    public async Task SelectTableAsync(int index)
    {
        await TableCards.Nth(index).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }
    
    public async Task SelectFirstAvailableTableAsync()
    {
        await SelectTableAsync(0);
    }

    #endregion

    #region Customer Information
    
    public async Task<bool> IsCustomerInfoFormVisibleAsync()
    {
        return await CustomerInfoHeader.IsVisibleAsync();
    }

    public async Task<string> GetCustomerNameAsync()
    {
        return await CustomerNameInput.InputValueAsync();
    }
    
    public async Task SetCustomerNameAsync(string name)
    {
        await CustomerNameInput.FillAsync(name);
    }
    
    public async Task<string> GetCustomerPhoneAsync()
    {
        return await CustomerPhoneInput.InputValueAsync();
    }
    
    public async Task SetCustomerPhoneAsync(string phone)
    {
        await CustomerPhoneInput.FillAsync(phone);
    }
    
    public async Task<string> GetCustomerEmailAsync()
    {
        return await CustomerEmailInput.InputValueAsync();
    }
    
    public async Task SetCustomerEmailAsync(string email)
    {
        await CustomerEmailInput.FillAsync(email);
    }
    
    public async Task<string> GetSpecialRequestsAsync()
    {
        return await SpecialRequestsTextarea.InputValueAsync();
    }
    
    public async Task SetSpecialRequestsAsync(string requests)
    {
        await SpecialRequestsTextarea.FillAsync(requests);
    }
    
    public async Task FillCustomerInfoAsync(string name, string phone, string email, string? specialRequests = null)
    {
        await SetCustomerNameAsync(name);
        await SetCustomerPhoneAsync(phone);
        await SetCustomerEmailAsync(email);
        
        if (!string.IsNullOrEmpty(specialRequests))
        {
            await SetSpecialRequestsAsync(specialRequests);
        }
    }
    
    public async Task<bool> IsCustomerInfoPrefilledAsync()
    {
        var name = await GetCustomerNameAsync();
        var email = await GetCustomerEmailAsync();
        return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email);
    }

    #endregion

    #region Confirm Reservation
    
    public async Task<bool> IsConfirmReservationButtonVisibleAsync()
    {
        return await ConfirmReservationButton.IsVisibleAsync();
    }
    
    public async Task<bool> IsProcessingReservationAsync()
    {
        return await ProcessingButton.IsVisibleAsync();
    }

    public async Task ClickConfirmReservationAsync()
    {
        await ConfirmReservationButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    public async Task WaitForReservationCompleteAsync()
    {
        await _page.WaitForSelectorAsync("button:has-text('Processing...')", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
    }

    #endregion

    #region Complete Booking Flow
    
    public async Task MakeReservationAsync(
        DateTime date,
        TimeOnly startTime,
        TimeOnly endTime,
        int guests,
        string customerName,
        string customerPhone,
        string customerEmail,
        string? specialRequests = null)
    {
        // Set date and time
        await SelectDateAsync(date);
        await SetStartTimeAsync(startTime);
        await SetEndTimeAsync(endTime);
        await SetGuestCountAsync(guests);

        // Check availability
        await ClickCheckAvailabilityAsync();
        await WaitForAvailabilityCheckAsync();

        // Verify tables are available
        var tableCount = await GetAvailableTableCountAsync();
        if (tableCount == 0)
        {
            throw new InvalidOperationException("No tables available for the selected date and time");
        }

        // Select first available table
        await SelectFirstAvailableTableAsync();

        // Fill customer info (if not pre-filled)
        if (!await IsCustomerInfoPrefilledAsync())
        {
            await FillCustomerInfoAsync(customerName, customerPhone, customerEmail, specialRequests);
        }
        else if (!string.IsNullOrEmpty(specialRequests))
        {
            await SetSpecialRequestsAsync(specialRequests);
        }

        // Confirm reservation
        await ClickConfirmReservationAsync();
        await WaitForReservationCompleteAsync();
    }
    
    public async Task<int> CheckAvailabilityForAsync(DateTime date, TimeOnly startTime, TimeOnly endTime, int guests)
    {
        await SelectDateAsync(date);
        await SetStartTimeAsync(startTime);
        await SetEndTimeAsync(endTime);
        await SetGuestCountAsync(guests);

        await ClickCheckAvailabilityAsync();
        await WaitForAvailabilityCheckAsync();

        return await GetAvailableTableCountAsync();
    }

    #endregion

    #region Assertions

    public async Task AssertBookingFormVisibleAsync()
    {
        await Assertions.Expect(BookingContainer).ToBeVisibleAsync();
        await Assertions.Expect(BookingHeader).ToBeVisibleAsync();
    }


    public async Task AssertAvailableTablesSectionVisibleAsync()
    {
        await Assertions.Expect(AvailableTablesSection).ToBeVisibleAsync();
        await Assertions.Expect(AvailableTablesHeader).ToBeVisibleAsync();
    }

    public async Task AssertCustomerInfoFormVisibleAsync()
    {
        await Assertions.Expect(CustomerInfoHeader).ToBeVisibleAsync();
        await Assertions.Expect(CustomerNameInput).ToBeVisibleAsync();
        await Assertions.Expect(CustomerEmailInput).ToBeVisibleAsync();
        await Assertions.Expect(CustomerPhoneInput).ToBeVisibleAsync();
    }
    
    public async Task AssertNoTablesAvailableAsync()
    {
        await Assertions.Expect(NoTablesAlert).ToBeVisibleAsync();
    }

    #endregion

    #region Toast Notifications
    
    public async Task<bool> IsSuccessToastVisibleAsync()
    {
        return await SuccessToast.IsVisibleAsync();
    }

    public async Task<bool> IsErrorToastVisibleAsync()
    {
        return await ErrorToast.IsVisibleAsync();
    }

    public async Task<string> GetSuccessToastMessageAsync()
    {
        await Assertions.Expect(SuccessToast).ToBeVisibleAsync(new() { Timeout = 5000 });
        return await SuccessToastBody.InnerTextAsync();
    }

    public async Task<string> GetErrorToastMessageAsync()
    {
        await Assertions.Expect(ErrorToast).ToBeVisibleAsync(new() { Timeout = 5000 });
        return await ErrorToastBody.InnerTextAsync();
    }

    public async Task WaitForSuccessToastAsync()
    {
        await Assertions.Expect(SuccessToast).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    public async Task WaitForErrorToastAsync()
    {
        await Assertions.Expect(ErrorToast).ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    #endregion
}