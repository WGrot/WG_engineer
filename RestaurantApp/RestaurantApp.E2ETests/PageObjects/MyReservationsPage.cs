using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;


public class MyReservationsPage
{
    private readonly IPage _page;
    private readonly string _baseUrl;

    public MyReservationsPage(IPage page, string baseUrl = "")
    {
        _page = page;
        _baseUrl = baseUrl;
    }

    #region Selectors

    // Header
    private ILocator PageHeader => _page.Locator(".card.bg-primary h2");
    private ILocator PageSubheader => _page.Locator(".card.bg-primary h5");

    // Loading states
    private ILocator InitialLoadingSpinner => _page.Locator(".spinner-border.text-primary").First;
    private ILocator LoadingText => _page.Locator("text=Loading your reservations...");

    // Empty states
    private ILocator NoReservationsAlert => _page.Locator(".alert.alert-info");
    private ILocator NoResultsMessage => _page.Locator("text=No reservations found matching your filters.");
    private ILocator AllReservationsLoadedMessage => _page.Locator("text=All reservations loaded");

    // Reservation list
    private ILocator ReservationCards => _page.Locator(".reservation-card");
    private ILocator LoadMoreButton => _page.Locator("button:has-text('Load more reservations')");
    private ILocator LoadMoreSpinner => LoadMoreButton.Locator(".spinner-border");

    // Page controls
    private ILocator ClearFiltersButton => _page.Locator("button:has-text('Clear Filters')");

    // Confirmation Modal
    private ILocator ModalContainer => _page.Locator(".modal").Filter(new() { HasText = "Confirm Cancellation" });
    private ILocator ModalConfirmButton => ModalContainer.Locator("button:has-text('Confirm Cancellation')");
    private ILocator ModalCancelButton => ModalContainer.Locator("button:has-text('Abort')");
    private ILocator ModalWarningIcon => ModalContainer.Locator(".bi-exclamation-triangle-fill");

    #endregion

    #region Navigation

    public async Task NavigateAsync()
    {
        await _page.GotoAsync($"{_baseUrl}/my-reservations");
        await WaitForPageLoadAsync();
    }

    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await WaitForInitialLoadingToCompleteAsync();
    }

    #endregion

    #region Loading States

    public async Task<bool> IsInitialLoadingVisibleAsync()
    {
        return await InitialLoadingSpinner.IsVisibleAsync();
    }

    public async Task WaitForInitialLoadingToCompleteAsync()
    {
        await InitialLoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 30000 });
    }

    public async Task<bool> IsLoadMoreInProgressAsync()
    {
        return await LoadMoreSpinner.IsVisibleAsync();
    }

    #endregion

    #region Page Header

    public async Task<string> GetPageHeaderTextAsync()
    {
        return await PageHeader.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetPageSubheaderTextAsync()
    {
        return await PageSubheader.TextContentAsync() ?? string.Empty;
    }

    #endregion

    #region Reservations List

    public async Task<int> GetReservationCountAsync()
    {
        return await ReservationCards.CountAsync();
    }

    public ReservationCard GetReservationCard(int index)
    {
        return new ReservationCard(ReservationCards.Nth(index));
    }

    public async Task<List<ReservationCard>> GetAllReservationCardsAsync()
    {
        var count = await GetReservationCountAsync();
        var cards = new List<ReservationCard>();

        for (int i = 0; i < count; i++)
        {
            cards.Add(GetReservationCard(i));
        }

        return cards;
    }

    public async Task<ReservationCard?> FindReservationByRestaurantNameAsync(string restaurantName)
    {
        var cards = await GetAllReservationCardsAsync();
        foreach (var card in cards)
        {
            var name = await card.GetRestaurantNameAsync();
            if (name.Contains(restaurantName, StringComparison.OrdinalIgnoreCase))
            {
                return card;
            }
        }
        return null;
    }

    #endregion

    #region Empty States

    public async Task<bool> HasNoReservationsAsync()
    {
        return await NoReservationsAlert.IsVisibleAsync();
    }

    public async Task<bool> HasNoFilterResultsAsync()
    {
        return await NoResultsMessage.IsVisibleAsync();
    }

    public async Task<bool> AreAllReservationsLoadedAsync()
    {
        return await AllReservationsLoadedMessage.IsVisibleAsync();
    }

    #endregion

    #region Load More

    public async Task<bool> IsLoadMoreButtonVisibleAsync()
    {
        return await LoadMoreButton.IsVisibleAsync();
    }

    public async Task ClickLoadMoreAsync()
    {
        await LoadMoreButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task LoadAllReservationsAsync(int maxIterations = 10)
    {
        int iterations = 0;
        while (await IsLoadMoreButtonVisibleAsync() && iterations < maxIterations)
        {
            await ClickLoadMoreAsync();
            iterations++;
        }
    }

    #endregion

    #region Sorting

    // Sort dropdown selectors (Bootstrap dropdown, not <select>)
    // Use parent .dropdown container that contains the "Sort by" button
    private ILocator SortDropdownContainer => _page.Locator(".dropdown:has(button:has-text('Sort by'))");
    private ILocator SortDropdownButton => SortDropdownContainer.Locator("button.dropdown-toggle");
    private ILocator SortDropdownMenu => SortDropdownContainer.Locator(".dropdown-menu");

    public async Task SortByAsync(string sortOption)
    {
        // Click dropdown button to open menu
        await SortDropdownButton.ClickAsync();
        
        // Wait for dropdown menu to be visible
        await SortDropdownMenu.WaitForAsync(new() { State = WaitForSelectorState.Visible });
        
        // Click the option by label text
        var optionLocator = SortDropdownMenu.Locator($".dropdown-item:has-text('{sortOption}')");
        await optionLocator.ClickAsync();
        
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task SortByNewestAsync() => await SortByAsync("Newest");
    public async Task SortByOldestAsync() => await SortByAsync("Oldest");
    public async Task SortByNextAsync() => await SortByAsync("Next");

    public async Task<string> GetCurrentSortOptionAsync()
    {
        var buttonText = await SortDropdownButton.TextContentAsync() ?? string.Empty;
        // Extract the selected option from "Sort by: {option}"
        return buttonText.Replace("Sort by:", "").Trim();
    }

    #endregion

    #region Filters

    // Filter selectors
    private ILocator StatusDropdown => _page.Locator("select.form-select").First;
    private ILocator DateInput => _page.Locator("input#date");
    private ILocator TimeFromInput => _page.Locator("input#startTime");
    private ILocator TimeToInput => _page.Locator("input#endTime");
    private ILocator ApplyFiltersButton => _page.Locator("button:has-text('Apply Filters')");
    private ILocator ClearAllButton => _page.Locator("button:has-text('Clear All')");

    public async Task ClearFiltersAsync()
    {
        await ClearAllButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task FilterByStatusAsync(string status)
    {
        await StatusDropdown.SelectOptionAsync(new SelectOptionValue { Label = status });
        await ApplyFiltersAsync();
    }

    public async Task FilterByDateAsync(DateTime date)
    {
        await DateInput.FillAsync(date.ToString("yyyy-MM-dd"));
        await ApplyFiltersAsync();
    }

    public async Task FilterByTimeRangeAsync(TimeSpan from, TimeSpan to)
    {
        await TimeFromInput.FillAsync(from.ToString(@"hh\:mm"));
        await TimeToInput.FillAsync(to.ToString(@"hh\:mm"));
        await ApplyFiltersAsync();
    }

    public async Task FilterByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        // Note: The component only has a single date field, not a range
        // This method sets the date and time range based on the dates provided
        await DateInput.FillAsync(startDate.ToString("yyyy-MM-dd"));
        await ApplyFiltersAsync();
    }

    public async Task ApplyFiltersAsync()
    {
        await ApplyFiltersButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task<string> GetSelectedStatusAsync()
    {
        return await StatusDropdown.InputValueAsync();
    }

    #endregion

    #region Cancellation Modal

    public async Task<bool> IsCancellationModalVisibleAsync()
    {
        return await ModalContainer.IsVisibleAsync();
    }

    public async Task<string> GetCancellationModalMessageAsync()
    {
        var messageElement = ModalContainer.Locator("p, .modal-body");
        return await messageElement.TextContentAsync() ?? string.Empty;
    }

    public async Task ConfirmCancellationAsync()
    {
        await ModalConfirmButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task AbortCancellationAsync()
    {
        await ModalCancelButton.ClickAsync();
    }

    public async Task WaitForModalToCloseAsync()
    {
        await ModalContainer.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    #endregion

    #region Toast/Success Messages

    public async Task<string> GetSuccessMessageAsync()
    {
        var successToast = _page.Locator(".toast-success, .alert-success, [class*='success']").First;
        await successToast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return await successToast.TextContentAsync() ?? string.Empty;
    }

    public async Task<string> GetErrorMessageAsync()
    {
        var errorToast = _page.Locator(".toast-error, .alert-danger, [class*='error']").First;
        await errorToast.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        return await errorToast.TextContentAsync() ?? string.Empty;
    }

    #endregion

    #region Page Size

    public async Task SetPageSizeAsync(int size)
    {
        var pageSizeSelector = _page.Locator($"button:has-text('{size}'), option:has-text('{size}')");
        await pageSizeSelector.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion

    #region Nested Component: ReservationCard

    public class ReservationCard
    {
        private readonly ILocator _card;

        public ReservationCard(ILocator card)
        {
            _card = card;
        }

        // Selectors
        private ILocator RestaurantName => _card.Locator(".bi-shop + span, .bi-shop ~ span").First;
        private ILocator StatusBadge => _card.Locator(".badge");
        private ILocator CustomerName => _card.Locator(".bi-person").Locator("..").First;
        private ILocator CustomerEmail => _card.Locator(".bi-envelope").Locator("..");
        private ILocator ReservationDate => _card.Locator(".bi-calendar-event").Locator("..");
        private ILocator ReservationTime => _card.Locator(".bi-clock").Locator("..");
        private ILocator GuestCount => _card.Locator(".bi-people").Locator("..");
        private ILocator TableInfo => _card.Locator(".bi-hash").Locator("..");
        private ILocator CancelButton => _card.Locator("button:has-text('Cancel')");

        // Actions
        public async Task ClickAsync()
        {
            await _card.Locator(".card-body").ClickAsync();
        }

        public async Task ClickCancelAsync()
        {
            await CancelButton.ClickAsync();
        }

        // Getters
        public async Task<string> GetRestaurantNameAsync()
        {
            return await RestaurantName.TextContentAsync() ?? string.Empty;
        }

        public async Task<string> GetStatusAsync()
        {
            return await StatusBadge.TextContentAsync() ?? string.Empty;
        }

        public async Task<string> GetStatusBadgeClassAsync()
        {
            return await StatusBadge.GetAttributeAsync("class") ?? string.Empty;
        }

        public async Task<string> GetCustomerNameAsync()
        {
            var text = await CustomerName.TextContentAsync() ?? string.Empty;
            return text.Trim();
        }

        public async Task<string> GetCustomerEmailAsync()
        {
            var text = await CustomerEmail.TextContentAsync() ?? string.Empty;
            return text.Trim();
        }

        public async Task<string> GetReservationDateAsync()
        {
            var text = await ReservationDate.TextContentAsync() ?? string.Empty;
            return text.Trim();
        }

        public async Task<string> GetReservationTimeAsync()
        {
            var text = await ReservationTime.TextContentAsync() ?? string.Empty;
            return text.Trim();
        }

        public async Task<string> GetGuestCountAsync()
        {
            var text = await GuestCount.TextContentAsync() ?? string.Empty;
            return text.Trim();
        }

        public async Task<bool> HasTableInfoAsync()
        {
            return await TableInfo.IsVisibleAsync();
        }

        public async Task<string> GetTableInfoAsync()
        {
            if (!await HasTableInfoAsync()) return string.Empty;
            return await TableInfo.TextContentAsync() ?? string.Empty;
        }

        // State checks
        public async Task<bool> IsCancelButtonVisibleAsync()
        {
            return await CancelButton.IsVisibleAsync();
        }

        public async Task<bool> IsConfirmedAsync()
        {
            var status = await GetStatusAsync();
            return status.Contains("Confirmed", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> IsPendingAsync()
        {
            var status = await GetStatusAsync();
            return status.Contains("Pending", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> IsCancelledAsync()
        {
            var status = await GetStatusAsync();
            return status.Contains("Cancelled", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<bool> CanBeCancelledAsync()
        {
            return await IsConfirmedAsync() || await IsPendingAsync();
        }
    }

    #endregion
}