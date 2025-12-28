// PageObjects/RestaurantDashboardPage.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class RestaurantDashboardPage
{
    private readonly IPage _page;

    public NextReservationsSection UpcomingReservations { get; }
    public NextReservationsSection PendingReservations { get; }
    public AvailableTablesSection AvailableTables { get; }
    public NewReservationModal NewReservationModal { get; }

    public RestaurantDashboardPage(IPage page)
    {
        _page = page;
        UpcomingReservations = new NextReservationsSection(page, "Next Reservations");
        PendingReservations = new NextReservationsSection(page, "Pending Reservations");
        AvailableTables = new AvailableTablesSection(page);
        NewReservationModal = new NewReservationModal(page);
    }

    // Locators - use more specific selectors
    // Main loading container is direct child before .container.mt-4
    private ILocator MainLoadingContainer => _page.Locator("body > app > main .loading-container").First;
    private ILocator MainContent => _page.Locator(".container.mt-4");
    private ILocator RestaurantNameHeader => _page.Locator(".restaurant-dropdown h4");
    private ILocator RestaurantDropdownButton => _page.Locator(".restaurant-dropdown .dropdown button");
    private ILocator RestaurantDropdownMenu => _page.Locator(".restaurant-dropdown .dropdown-menu");
    private ILocator NewReservationButton => _page.Locator("button:has-text('New Reservation')");
    
    // Statistics cards
    private ILocator TodayReservationsCard => _page.Locator(".card:has(h6:has-text('Today reservations'))");
    private ILocator AvailableTablesCard => _page.Locator(".card:has(h6:has-text('Available Tables'))");
    private ILocator AvailableSeatsCard => _page.Locator(".card:has(h6:has-text('Available Seats'))");
    private ILocator LastWeekReservationsCard => _page.Locator(".card:has(h6:has-text('Reservations in last week'))");

    // Navigation
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/RestaurantDashboard");
        await WaitForLoadAsync();
    }

    public async Task WaitForLoadAsync()
    {
        // Wait for main content to appear (this means main loading is done)
        await MainContent.WaitForAsync(new() { Timeout = 15000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task WaitForAllSectionsToLoadAsync()
    {
        // Wait for main content
        await WaitForLoadAsync();
        
        // Wait for all loading containers to disappear
        var allLoadingContainers = _page.Locator(".loading-container");
        var count = await allLoadingContainers.CountAsync();
        
        for (int i = 0; i < count; i++)
        {
            try
            {
                await allLoadingContainers.Nth(i).WaitForAsync(
                    new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
            }
            catch (TimeoutException) { }
        }
        
        await _page.WaitForTimeoutAsync(300);
    }

    // Alternative approach - check if main page is loading
    public async Task<bool> IsLoadingAsync()
    {
        // Main page is loading if content container is not visible
        return !await MainContent.IsVisibleAsync();
    }

    // Restaurant selection
    public async Task<string> GetCurrentRestaurantNameAsync()
    {
        return (await RestaurantNameHeader.InnerTextAsync()).Trim();
    }

    public async Task<bool> CanSwitchRestaurantAsync()
    {
        return await RestaurantDropdownButton.IsVisibleAsync();
    }

    public async Task SwitchRestaurantAsync(string restaurantName)
    {
        await RestaurantDropdownButton.ClickAsync();
        await RestaurantDropdownMenu.WaitForAsync(new() { Timeout = 3000 });
        await RestaurantDropdownMenu.Locator($"button:has-text('{restaurantName}')").ClickAsync();
        await WaitForLoadAsync();
    }

    public async Task<List<string>> GetAvailableRestaurantsAsync()
    {
        var restaurants = new List<string>();
        
        if (!await CanSwitchRestaurantAsync())
            return restaurants;

        await RestaurantDropdownButton.ClickAsync();
        await RestaurantDropdownMenu.WaitForAsync(new() { Timeout = 3000 });

        var items = RestaurantDropdownMenu.Locator("button.dropdown-item");
        var count = await items.CountAsync();

        for (int i = 0; i < count; i++)
        {
            restaurants.Add((await items.Nth(i).InnerTextAsync()).Trim());
        }

        // Close dropdown
        await _page.Keyboard.PressAsync("Escape");
        return restaurants;
    }

    // New Reservation
    public async Task OpenNewReservationModalAsync()
    {
        await NewReservationButton.ClickAsync();
        await NewReservationModal.WaitForVisibleAsync();
    }

    // Statistics
    public async Task<string> GetTodayReservationsCountAsync()
    {
        var valueElement = TodayReservationsCard.Locator("h2 div");
        return (await valueElement.InnerTextAsync()).Trim();
    }

    public async Task<string> GetAvailableTablesCountAsync()
    {
        var valueElement = AvailableTablesCard.Locator("h2");
        return (await valueElement.InnerTextAsync()).Trim();
    }

    public async Task<string> GetAvailableSeatsCountAsync()
    {
        var valueElement = AvailableSeatsCard.Locator("h2");
        return (await valueElement.InnerTextAsync()).Trim();
    }

    public async Task<string> GetLastWeekReservationsCountAsync()
    {
        var valueElement = LastWeekReservationsCard.Locator("h2 div");
        return (await valueElement.InnerTextAsync()).Trim();
    }

    public async Task<DashboardStatistics> GetAllStatisticsAsync()
    {
        return new DashboardStatistics
        {
            TodayReservations = await GetTodayReservationsCountAsync(),
            AvailableTables = await GetAvailableTablesCountAsync(),
            AvailableSeats = await GetAvailableSeatsCountAsync(),
            LastWeekReservations = await GetLastWeekReservationsCountAsync()
        };
    }
}

public record DashboardStatistics
{
    public string TodayReservations { get; init; } = "";
    public string AvailableTables { get; init; } = "";
    public string AvailableSeats { get; init; } = "";
    public string LastWeekReservations { get; init; } = "";
}