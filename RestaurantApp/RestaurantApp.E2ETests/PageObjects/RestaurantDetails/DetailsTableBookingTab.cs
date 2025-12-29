using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class DetailsTableBookingTab
{
    private readonly IPage _page;

    public DetailsTableBookingTab(IPage page)
    {
        _page = page;
    }

    // Main container
    private ILocator BookingContainer => _page.Locator(".booking-container");

    // Loading state
    private ILocator LoadingSpinner => _page.Locator(".booking-container .loading-container .spinner");
    private ILocator LoadingText => _page.Locator(".booking-container .loading-container p:has-text('Loading')");

    // Error state (restaurant not found)
    private ILocator ErrorContainer => _page.Locator(".booking-container .error");
    private ILocator GoBackButton => _page.Locator(".booking-container .error button:has-text('Go Back')");

    // Header
    private ILocator BookingHeader => _page.Locator("h3:has-text('Check table availability')");

    // Date selection
    private ILocator PreviousDayButton => _page.Locator(".arrow-btn:has(.bi-chevron-left)");
    private ILocator NextDayButton => _page.Locator(".arrow-btn:has(.bi-chevron-right)");
    private ILocator SelectedDayName => _page.Locator(".d-flex.flex-column.align-items-center strong");
    private ILocator DateInput => _page.Locator("input[type='date']");

    // Time selection
    private ILocator StartTimeInput => _page.Locator("#startTime");
    private ILocator EndTimeInput => _page.Locator("#endTime");

    // Guest count
    private ILocator GuestCountInput => _page.Locator("#guests");

    // Check availability button
    private ILocator CheckAvailabilityButton => _page.Locator("button:has-text('Check Available Tables')");
    private ILocator CheckingAvailabilityButton => _page.Locator("button:has-text('Checking...')");

    // Available tables section
    private ILocator AvailableTablesSection => _page.Locator(".available-tables");
    private ILocator AvailableTablesHeader => _page.Locator(".available-tables h3:has-text('Available Tables')");
    private ILocator NoTablesAlert => _page.Locator(".alert-info:has-text('No tables available')");
    private ILocator TableCards => _page.Locator(".available-tables .row .col-12, .available-tables .row .col-sm-6, .available-tables .row .col-md-4, .available-tables .row .col-lg-3");

    // Customer information form
    private ILocator CustomerInfoHeader => _page.Locator("h4:has-text('Customer Information')");
    private ILocator CustomerNameInput => _page.Locator("#name");
    private ILocator CustomerPhoneInput => _page.Locator("#phone");
    private ILocator CustomerEmailInput => _page.Locator("#email");
    private ILocator SpecialRequestsTextarea => _page.Locator("#notes");

    // Confirm reservation button
    private ILocator ConfirmReservationButton => _page.Locator("button:has-text('Confirm Reservation')");
    private ILocator ProcessingButton => _page.Locator("button:has-text('Processing...')");

    #region Loading State

    /// <summary>
    /// Check if form is loading restaurant data
    /// </summary>
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    /// <summary>
    /// Wait for form to load
    /// </summary>
    public async Task WaitForFormLoadAsync()
    {
        await _page.WaitForSelectorAsync(".booking-container .loading-container", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
    }

    /// <summary>
    /// Check if restaurant not found error is displayed
    /// </summary>
    public async Task<bool> IsRestaurantNotFoundAsync()
    {
        return await ErrorContainer.IsVisibleAsync();
    }

    /// <summary>
    /// Click go back button on error page
    /// </summary>
    public async Task ClickGoBackAsync()
    {
        await GoBackButton.ClickAsync();
    }

    #endregion

    #region Date Selection

    /// <summary>
    /// Get currently selected date
    /// </summary>
    public async Task<DateTime> GetSelectedDateAsync()
    {
        var dateString = await DateInput.InputValueAsync();
        return DateTime.Parse(dateString);
    }

    /// <summary>
    /// Get displayed day name (e.g., "Monday")
    /// </summary>
    public async Task<string> GetSelectedDayNameAsync()
    {
        return await SelectedDayName.InnerTextAsync();
    }

    /// <summary>
    /// Select specific date
    /// </summary>
    public async Task SelectDateAsync(DateTime date)
    {
        var dateString = date.ToString("yyyy-MM-dd");
        await DateInput.FillAsync(dateString);
        await DateInput.DispatchEventAsync("change");
        await _page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Navigate to previous day
    /// </summary>
    public async Task ClickPreviousDayAsync()
    {
        await PreviousDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(200);
    }

    /// <summary>
    /// Navigate to next day
    /// </summary>
    public async Task ClickNextDayAsync()
    {
        await NextDayButton.ClickAsync();
        await _page.WaitForTimeoutAsync(200);
    }

    #endregion

    #region Time Selection

    /// <summary>
    /// Get start time value
    /// </summary>
    public async Task<string> GetStartTimeAsync()
    {
        return await StartTimeInput.InputValueAsync();
    }

    /// <summary>
    /// Set start time
    /// </summary>
    public async Task SetStartTimeAsync(TimeOnly time)
    {
        var timeString = time.ToString("HH:mm");
        await StartTimeInput.FillAsync(timeString);
    }

    /// <summary>
    /// Set start time using string format
    /// </summary>
    public async Task SetStartTimeAsync(string time)
    {
        await StartTimeInput.FillAsync(time);
    }

    /// <summary>
    /// Get end time value
    /// </summary>
    public async Task<string> GetEndTimeAsync()
    {
        return await EndTimeInput.InputValueAsync();
    }

    /// <summary>
    /// Set end time
    /// </summary>
    public async Task SetEndTimeAsync(TimeOnly time)
    {
        var timeString = time.ToString("HH:mm");
        await EndTimeInput.FillAsync(timeString);
    }

    /// <summary>
    /// Set end time using string format
    /// </summary>
    public async Task SetEndTimeAsync(string time)
    {
        await EndTimeInput.FillAsync(time);
    }

    #endregion

    #region Guest Count

    /// <summary>
    /// Get number of guests
    /// </summary>
    public async Task<int> GetGuestCountAsync()
    {
        var value = await GuestCountInput.InputValueAsync();
        return int.Parse(value);
    }

    /// <summary>
    /// Set number of guests
    /// </summary>
    public async Task SetGuestCountAsync(int count)
    {
        await GuestCountInput.FillAsync(count.ToString());
    }

    #endregion

    #region Check Availability

    /// <summary>
    /// Check if "Check Available Tables" button is visible
    /// </summary>
    public async Task<bool> IsCheckAvailabilityButtonVisibleAsync()
    {
        return await CheckAvailabilityButton.IsVisibleAsync();
    }

    /// <summary>
    /// Check if availability check is in progress
    /// </summary>
    public async Task<bool> IsCheckingAvailabilityAsync()
    {
        return await CheckingAvailabilityButton.IsVisibleAsync();
    }

    /// <summary>
    /// Click "Check Available Tables" button
    /// </summary>
    public async Task ClickCheckAvailabilityAsync()
    {
        await CheckAvailabilityButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Wait for availability check to complete
    /// </summary>
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

    /// <summary>
    /// Check if available tables section is displayed
    /// </summary>
    public async Task<bool> IsAvailableTablesSectionVisibleAsync()
    {
        return await AvailableTablesSection.IsVisibleAsync();
    }

    /// <summary>
    /// Check if no tables available message is displayed
    /// </summary>
    public async Task<bool> IsNoTablesAvailableAsync()
    {
        return await NoTablesAlert.IsVisibleAsync();
    }

    /// <summary>
    /// Get count of available tables
    /// </summary>
    public async Task<int> GetAvailableTableCountAsync()
    {
        if (await IsNoTablesAvailableAsync())
            return 0;

        return await TableCards.CountAsync();
    }

    /// <summary>
    /// Select table by index
    /// </summary>
    public async Task SelectTableAsync(int index)
    {
        await TableCards.Nth(index).ClickAsync();
        await _page.WaitForTimeoutAsync(300);
    }

    /// <summary>
    /// Select first available table
    /// </summary>
    public async Task SelectFirstAvailableTableAsync()
    {
        await SelectTableAsync(0);
    }

    #endregion

    #region Customer Information

    /// <summary>
    /// Check if customer information form is visible
    /// </summary>
    public async Task<bool> IsCustomerInfoFormVisibleAsync()
    {
        return await CustomerInfoHeader.IsVisibleAsync();
    }

    /// <summary>
    /// Get customer name value
    /// </summary>
    public async Task<string> GetCustomerNameAsync()
    {
        return await CustomerNameInput.InputValueAsync();
    }

    /// <summary>
    /// Set customer name
    /// </summary>
    public async Task SetCustomerNameAsync(string name)
    {
        await CustomerNameInput.FillAsync(name);
    }

    /// <summary>
    /// Get customer phone value
    /// </summary>
    public async Task<string> GetCustomerPhoneAsync()
    {
        return await CustomerPhoneInput.InputValueAsync();
    }

    /// <summary>
    /// Set customer phone
    /// </summary>
    public async Task SetCustomerPhoneAsync(string phone)
    {
        await CustomerPhoneInput.FillAsync(phone);
    }

    /// <summary>
    /// Get customer email value
    /// </summary>
    public async Task<string> GetCustomerEmailAsync()
    {
        return await CustomerEmailInput.InputValueAsync();
    }

    /// <summary>
    /// Set customer email
    /// </summary>
    public async Task SetCustomerEmailAsync(string email)
    {
        await CustomerEmailInput.FillAsync(email);
    }

    /// <summary>
    /// Get special requests value
    /// </summary>
    public async Task<string> GetSpecialRequestsAsync()
    {
        return await SpecialRequestsTextarea.InputValueAsync();
    }

    /// <summary>
    /// Set special requests
    /// </summary>
    public async Task SetSpecialRequestsAsync(string requests)
    {
        await SpecialRequestsTextarea.FillAsync(requests);
    }

    /// <summary>
    /// Fill all customer information fields
    /// </summary>
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

    /// <summary>
    /// Check if customer info is pre-filled (for logged-in users)
    /// </summary>
    public async Task<bool> IsCustomerInfoPrefilledAsync()
    {
        var name = await GetCustomerNameAsync();
        var email = await GetCustomerEmailAsync();
        return !string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(email);
    }

    #endregion

    #region Confirm Reservation

    /// <summary>
    /// Check if "Confirm Reservation" button is visible
    /// </summary>
    public async Task<bool> IsConfirmReservationButtonVisibleAsync()
    {
        return await ConfirmReservationButton.IsVisibleAsync();
    }

    /// <summary>
    /// Check if reservation is being processed
    /// </summary>
    public async Task<bool> IsProcessingReservationAsync()
    {
        return await ProcessingButton.IsVisibleAsync();
    }

    /// <summary>
    /// Click "Confirm Reservation" button
    /// </summary>
    public async Task ClickConfirmReservationAsync()
    {
        await ConfirmReservationButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Wait for reservation to complete
    /// </summary>
    public async Task WaitForReservationCompleteAsync()
    {
        await _page.WaitForSelectorAsync("button:has-text('Processing...')", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
    }

    #endregion

    #region Complete Booking Flow

    /// <summary>
    /// Complete the entire booking process
    /// </summary>
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

    /// <summary>
    /// Quick check availability for given parameters
    /// </summary>
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

    /// <summary>
    /// Assert booking form is visible
    /// </summary>
    public async Task AssertBookingFormVisibleAsync()
    {
        await Assertions.Expect(BookingContainer).ToBeVisibleAsync();
        await Assertions.Expect(BookingHeader).ToBeVisibleAsync();
    }

    /// <summary>
    /// Assert available tables section is visible
    /// </summary>
    public async Task AssertAvailableTablesSectionVisibleAsync()
    {
        await Assertions.Expect(AvailableTablesSection).ToBeVisibleAsync();
        await Assertions.Expect(AvailableTablesHeader).ToBeVisibleAsync();
    }

    /// <summary>
    /// Assert customer info form is visible
    /// </summary>
    public async Task AssertCustomerInfoFormVisibleAsync()
    {
        await Assertions.Expect(CustomerInfoHeader).ToBeVisibleAsync();
        await Assertions.Expect(CustomerNameInput).ToBeVisibleAsync();
        await Assertions.Expect(CustomerEmailInput).ToBeVisibleAsync();
        await Assertions.Expect(CustomerPhoneInput).ToBeVisibleAsync();
    }

    /// <summary>
    /// Assert no tables available message is shown
    /// </summary>
    public async Task AssertNoTablesAvailableAsync()
    {
        await Assertions.Expect(NoTablesAlert).ToBeVisibleAsync();
    }

    #endregion
}