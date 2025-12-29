using Microsoft.Playwright;
using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantDetails;

public class RestaurantDetailsPage
{
    private readonly IPage _page;

    public DetailsInfoTab Info { get; }
    public DetailsMenuTab Menu { get; }
    public DetailsTablesTab Tables { get; }
    public DetailsTableBookingTab TableBooking { get; }
    public ReviewsTab Reviews { get; }

    public RestaurantDetailsPage(IPage page)
    {
        _page = page;
        Info = new DetailsInfoTab(page);
        Menu = new DetailsMenuTab(page);
        Tables = new DetailsTablesTab(page);
        TableBooking = new DetailsTableBookingTab(page);
        Reviews = new ReviewsTab(page);
    }

    private ILocator RestaurantHeader => _page.Locator(".restaurant-header");
    private ILocator RestaurantName => _page.Locator(".restaurant-header h1");
    private ILocator RestaurantAddress => _page.Locator(".restaurant-header .opacity-75");
    private ILocator RestaurantLogo => _page.Locator(".restaurant-header .restaurant-logo, .restaurant-header .restaurant-logo-placeholder");
    private ILocator BookTableHeaderButton => _page.Locator(".restaurant-header .base-button");
    private ILocator LoadingSpinner => _page.Locator(".loading-container .spinner");
    private ILocator LoadingText => _page.Locator(".loading-container p");

    private ILocator TabButtons => _page.Locator(".nav-tabs .nav-link");
    private ILocator ActiveTabButton => _page.Locator(".nav-tabs .nav-link.active");
    private ILocator InfoTabButton => _page.Locator(".nav-tabs .nav-link:has-text('Info')");
    private ILocator MenuTabButton => _page.Locator(".nav-tabs .nav-link:has-text('Menu')");
    private ILocator TablesTabButton => _page.Locator(".nav-tabs .nav-link:has-text('Tables')");
    private ILocator TableBookingTabButton => _page.Locator(".nav-tabs .nav-link:has-text('Table Booking')");
    private ILocator ReviewsTabButton => _page.Locator(".nav-tabs .nav-link:has-text('Reviews')");

    private ILocator TabContent => _page.Locator(".tab-content");
    private ILocator ActiveTabPane => _page.Locator(".tab-pane.show.active");
    
    public async Task GotoAsync(int restaurantId)
    {
        await _page.GotoAsync($"/restaurant/{restaurantId}");
        await WaitForPageLoadAsync();
    }
    
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForSelectorAsync(".loading-container", 
            new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await Assertions.Expect(RestaurantHeader).ToBeVisibleAsync();
    }

    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    #region Header Information

    public async Task<string> GetRestaurantNameAsync()
    {
        return await RestaurantName.InnerTextAsync();
    }

    public async Task<string> GetRestaurantAddressAsync()
    {
        var text = await RestaurantAddress.InnerTextAsync();
        return text.Trim();
    }

    public async Task<bool> IsLogoDisplayedAsync()
    {
        return await RestaurantLogo.IsVisibleAsync();
    }

    public async Task ClickBookTableInHeaderAsync()
    {
        await BookTableHeaderButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #endregion

    #region Tab Navigation

    public async Task SwitchToInfoTabAsync()
    {
        await InfoTabButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task SwitchToMenuTabAsync()
    {
        await MenuTabButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task SwitchToTablesTabAsync()
    {
        await TablesTabButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task SwitchToTableBookingTabAsync()
    {
        await TableBookingTabButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    public async Task SwitchToReviewsTabAsync()
    {
        await ReviewsTabButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }


    public async Task<string> GetActiveTabNameAsync()
    {
        var text = await ActiveTabButton.InnerTextAsync();
        return text.Trim();
    }

    public async Task<List<string>> GetVisibleTabNamesAsync()
    {
        var names = new List<string>();
        var count = await TabButtons.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var text = await TabButtons.Nth(i).InnerTextAsync();
            names.Add(text.Trim());
        }

        return names;
    }
    
    public async Task<bool> IsTabVisibleAsync(string tabName)
    {
        var tabButton = _page.Locator($".nav-tabs .nav-link:has-text('{tabName}')");
        return await tabButton.IsVisibleAsync();
    }
    
    public async Task<bool> IsTabActiveAsync(string tabName)
    {
        var activeTab = await GetActiveTabNameAsync();
        return activeTab.Contains(tabName, StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Assertions
    
    public async Task AssertHeaderVisibleAsync()
    {
        await Assertions.Expect(RestaurantHeader).ToBeVisibleAsync();
        await Assertions.Expect(RestaurantName).ToBeVisibleAsync();
        await Assertions.Expect(RestaurantAddress).ToBeVisibleAsync();
    }

    public async Task AssertAllTabsVisibleAsync()
    {
        await Assertions.Expect(InfoTabButton).ToBeVisibleAsync();
        await Assertions.Expect(MenuTabButton).ToBeVisibleAsync();
        await Assertions.Expect(TablesTabButton).ToBeVisibleAsync();
        await Assertions.Expect(TableBookingTabButton).ToBeVisibleAsync();
        await Assertions.Expect(ReviewsTabButton).ToBeVisibleAsync();
    }

    #endregion
}