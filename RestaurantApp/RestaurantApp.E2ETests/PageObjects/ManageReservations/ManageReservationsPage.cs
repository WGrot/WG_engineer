// PageObjects/ManageReservationsPage.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class ManageReservationsPage
{
    private readonly IPage _page;
    private const string PageUrl = "/manage-reservations";

    public ManageReservationsPage(IPage page) => _page = page;

    // Page header
    private ILocator PageTitle => _page.Locator("h2:has-text('Manage Reservations')");
    private ILocator PageDescription => _page.Locator("p.text-muted:has-text('View and manage all restaurant reservations')");

    // Loading states
    private ILocator InitialLoadingSpinner => _page.Locator(".spinner-border.text-primary");
    private ILocator LoadingText => _page.Locator("p.text-muted:has-text('Loading reservations...')");

    // Filters panel
    private ILocator FiltersCard => _page.Locator(".card:has(.card-header:has-text('Filters'))");
    private ILocator CustomerNameInput => FiltersCard.Locator("input[placeholder='Enter name...']");
    private ILocator CustomerEmailInput => FiltersCard.Locator("input[placeholder='Enter email...']");
    private ILocator CustomerPhoneInput => FiltersCard.Locator("input[placeholder='Enter phone...']");
    private ILocator RestaurantNameInput => FiltersCard.Locator("input[placeholder='Enter Restaurant name']");
    private ILocator StatusSelect => FiltersCard.Locator("select.form-select");
    private ILocator ReservationDateInput => FiltersCard.Locator("input#date[type='date']");
    private ILocator TimeFromInput => FiltersCard.Locator("input#startTime[type='time']");
    private ILocator TimeToInput => FiltersCard.Locator("input#endTime[type='time']");
    private ILocator ApplyFiltersButton => FiltersCard.Locator("button:has-text('Apply Filters')");
    private ILocator ClearFiltersButton => FiltersCard.Locator("button:has-text('Clear All')");

    // Page size and sort controls
    private ILocator PageSizeDropdown => _page.Locator("button.dropdown-toggle:has-text('Reservations per page')");
    private ILocator SortDropdown => _page.Locator("button.dropdown-toggle:has-text('Sort by')");

    // Reservation cards
    private ILocator ReservationCards => _page.Locator(".reservation-card");
    private ILocator LoadMoreButton => _page.Locator("button:has-text('Load more reservations')");
    private ILocator AllLoadedText => _page.Locator(".text-muted:has-text('All reservations loaded')");
    private ILocator NoReservationsText => _page.Locator("p.text-muted:has-text('No reservations found matching your filters')");
    private ILocator NoResultsClearFiltersButton => _page.Locator("button:has-text('Clear Filters')");

    // Navigation
    public async Task GotoAsync()
    {
        await _page.GotoAsync(PageUrl);
        await WaitForPageLoadAsync();
    }

    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await PageTitle.WaitForAsync(new() { Timeout = 10000 });
    }

    public async Task WaitForReservationsLoadedAsync()
    {
        // Wait for initial loading spinner to disappear
        await InitialLoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 15000 });
    }

    // Page state checks
    public async Task<bool> IsPageTitleVisibleAsync() => await PageTitle.IsVisibleAsync();
    public async Task<bool> IsPageDescriptionVisibleAsync() => await PageDescription.IsVisibleAsync();
    public async Task<bool> IsLoadingAsync() => await InitialLoadingSpinner.IsVisibleAsync();
    public async Task<bool> IsFiltersCardVisibleAsync() => await FiltersCard.IsVisibleAsync();

    // Filters visibility
    public async Task<bool> IsCustomerNameFilterVisibleAsync() => await CustomerNameInput.IsVisibleAsync();
    public async Task<bool> IsCustomerEmailFilterVisibleAsync() => await CustomerEmailInput.IsVisibleAsync();
    public async Task<bool> IsCustomerPhoneFilterVisibleAsync() => await CustomerPhoneInput.IsVisibleAsync();
    public async Task<bool> IsRestaurantNameFilterVisibleAsync() => await RestaurantNameInput.IsVisibleAsync();
    public async Task<bool> IsStatusFilterVisibleAsync() => await StatusSelect.IsVisibleAsync();
    public async Task<bool> IsDateFilterVisibleAsync() => await ReservationDateInput.IsVisibleAsync();
    public async Task<bool> IsTimeRangeFiltersVisibleAsync() => 
        await TimeFromInput.IsVisibleAsync() && await TimeToInput.IsVisibleAsync();
    public async Task<bool> IsApplyFiltersButtonVisibleAsync() => await ApplyFiltersButton.IsVisibleAsync();
    public async Task<bool> IsClearFiltersButtonVisibleAsync() => await ClearFiltersButton.IsVisibleAsync();

    // Controls visibility
    public async Task<bool> IsPageSizeDropdownVisibleAsync() => await PageSizeDropdown.IsVisibleAsync();
    public async Task<bool> IsSortDropdownVisibleAsync() => await SortDropdown.IsVisibleAsync();

    // Reservations list
    public async Task<int> GetReservationCountAsync() => await ReservationCards.CountAsync();
    public async Task<bool> HasReservationsAsync() => await ReservationCards.CountAsync() > 0;
    public async Task<bool> IsLoadMoreButtonVisibleAsync() => await LoadMoreButton.IsVisibleAsync();
    
    public ILocator SuccessToast => _page.Locator(".toast.border-success");
    
    public ILocator ErrorToast => _page.Locator(".toast.border-danger");
    
    public ILocator SuccessToastBody => _page.Locator(".toast.border-success .toast-body");
    
    public ILocator ErrorToastBody => _page.Locator(".toast.border-danger .toast-body");

    // Filter actions
    public async Task FillCustomerNameFilterAsync(string name)
    {
        await CustomerNameInput.FillAsync(name);
    }

    public async Task FillCustomerEmailFilterAsync(string email)
    {
        await CustomerEmailInput.FillAsync(email);
    }

    public async Task FillCustomerPhoneFilterAsync(string phone)
    {
        await CustomerPhoneInput.FillAsync(phone);
    }

    public async Task FillRestaurantNameFilterAsync(string name)
    {
        await RestaurantNameInput.FillAsync(name);
    }

    public async Task SelectStatusAsync(string status)
    {
        await StatusSelect.SelectOptionAsync(status);
    }

    public async Task FillReservationDateAsync(string date)
    {
        await ReservationDateInput.FillAsync(date);
    }

    public async Task FillTimeRangeAsync(string from, string to)
    {
        await TimeFromInput.FillAsync(from);
        await TimeToInput.FillAsync(to);
    }

    public async Task ApplyFiltersAsync()
    {
        await ApplyFiltersButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task ClearFiltersAsync()
    {
        await ClearFiltersButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }
    
    public async Task<bool> IsNoReservationsMessageVisibleAsync() => await NoReservationsText.IsVisibleAsync();

    // Page size actions
    public async Task SetPageSizeAsync(int size)
    {
        await PageSizeDropdown.ClickAsync();
        await _page.Locator($".dropdown-menu .dropdown-item:has-text('{size}')").First.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // Sort actions
    public async Task SelectSortOptionAsync(string optionLabel)
    {
        await SortDropdown.ClickAsync();
        await _page.Locator($".dropdown-menu .dropdown-item:has-text('{optionLabel}')").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // Load more
    public async Task LoadMoreReservationsAsync()
    {
        await LoadMoreButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // Reservation card interactions
    public async Task<ReservationCardData> GetReservationCardDataAsync(int index)
    {
        var card = ReservationCards.Nth(index);
        
        var data = new ReservationCardData
        {
            RestaurantName = await GetTextSafeAsync(card.Locator(".bi-shop + span")),
            Status = await GetTextSafeAsync(card.Locator(".badge").First),
            CustomerName = await GetTextSafeAsync(card.Locator(".bi-person").Locator("..").First),
            CustomerEmail = await GetTextSafeAsync(card.Locator(".bi-envelope").Locator("..").First),
            Date = await GetTextSafeAsync(card.Locator(".bi-calendar-event").Locator("..")),
            Time = await GetTextSafeAsync(card.Locator(".bi-clock").Locator("..")),
            Guests = await GetTextSafeAsync(card.Locator(".bi-people").Locator(".."))
        };

        var tableNumberLocator = card.Locator(".bi-123").Locator("..");
        if (await tableNumberLocator.IsVisibleAsync())
        {
            data.TableNumber = await GetTextSafeAsync(tableNumberLocator);
        }

        return data;
    }

    public async Task<List<ReservationCardData>> GetAllReservationCardsDataAsync()
    {
        var count = await GetReservationCountAsync();
        var cards = new List<ReservationCardData>();
        
        for (int i = 0; i < count; i++)
        {
            cards.Add(await GetReservationCardDataAsync(i));
        }
        
        return cards;
    }

    public async Task ClickReservationCardAsync(int index)
    {
        await ReservationCards.Nth(index).ClickAsync();
    }

    public async Task<bool> HasApproveButtonOnCardAsync(int index)
    {
        var card = ReservationCards.Nth(index);
        return await card.Locator(".badge:has-text('Approve')").IsVisibleAsync();
    }

    public async Task ClickApproveOnCardAsync(int index)
    {
        var card = ReservationCards.Nth(index);
        await card.Locator(".badge:has-text('Approve')").ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    private async Task<string> GetTextSafeAsync(ILocator locator)
    {
        if (!await locator.IsVisibleAsync())
            return "";
        return (await locator.InnerTextAsync()).Trim();
    }
}

public record ReservationCardData
{
    public string RestaurantName { get; set; } = "";
    public string Status { get; set; } = "";
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string Guests { get; set; } = "";
    public string? TableNumber { get; set; }
}