using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.RestaurantSearch;

public class RestaurantsListPage
{
    private readonly IPage _page;
    private readonly string _url = "/Restaurants";


    private ILocator PageTitle => _page.Locator(".search-header h1");
    private ILocator SearchNameInput => _page.Locator(".filter-input-group input[placeholder='Search restaurants...']");
    private ILocator SearchLocationInput => _page.Locator(".filter-input-group input[placeholder='Location...']");
    private ILocator SearchButton => _page.Locator(".filter-button");
    
    private ILocator LoadingSpinner => _page.Locator(".loading-container .spinner");
    
    private ILocator LoadingText => _page.Locator(".loading-container p");
    
    private ILocator NoResultsContainer => _page.Locator(".no-results");
    private ILocator NoResultsTitle => _page.Locator(".no-results h3");
    private ILocator ClearFiltersButton => _page.Locator(".no-results .btn-primary");


    private ILocator ResultsInfo => _page.Locator(".results-info p");

    private ILocator SortDropdownButton => _page.Locator("button:has-text('Sort by:')");
    private ILocator SortOptionAZ => _page.Locator(".dropdown-item:has-text('A-z')");
    private ILocator SortOptionZA => _page.Locator(".dropdown-item:has-text('Z-a')");
    private ILocator SortOptionHighestRating => _page.Locator(".dropdown-item:has-text('Highest Rating')");
    private ILocator SortOptionLowestRating => _page.Locator(".dropdown-item:has-text('Lowest Rating')");

    private ILocator PageSizeDropdownButton => _page.Locator("button:has-text('Restaurants per page:')");
    private ILocator PageSizeOption10 => _page.Locator(".dropdown-menu .dropdown-item:has-text('10')");
    private ILocator PageSizeOption25 => _page.Locator(".dropdown-menu .dropdown-item:has-text('25')");
    private ILocator PageSizeOption50 => _page.Locator(".dropdown-menu .dropdown-item:has-text('50')");

    private ILocator RestaurantCards => _page.Locator(".restaurant-card");
    private ILocator RestaurantNames => _page.Locator(".restaurant-card .restaurant-name");
    private ILocator RestaurantAddresses => _page.Locator(".restaurant-card .detail-item:has(.bi-geo-alt-fill) span");


    private ILocator LoadMoreButton => _page.Locator(".btn-outline-primary:has-text('Load More')");
    private ILocator LoadMoreSpinner => _page.Locator(".btn-outline-primary .spinner-border");
    private ILocator AllLoadedMessage => _page.Locator("text=All Restaurants loaded");

    public RestaurantsListPage(IPage page)
    {
        _page = page;
    }

    public async Task GotoAsync()
    {
        await _page.GotoAsync(_url);
    }

    public async Task GotoWithSearchAsync(string? name = null, string? location = null)
    {
        var queryParams = new List<string>();

        if (!string.IsNullOrWhiteSpace(name))
            queryParams.Add($"name={Uri.EscapeDataString(name)}");

        if (!string.IsNullOrWhiteSpace(location))
            queryParams.Add($"location={Uri.EscapeDataString(location)}");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        await _page.GotoAsync($"{_url}{queryString}");
    }
    
    public async Task WaitForPageLoadAsync()
    {
        await Assertions.Expect(PageTitle).ToBeVisibleAsync();
        await Assertions.Expect(LoadingSpinner).Not.ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    public async Task WaitForRestaurantsLoadedAsync()
    {
        await WaitForPageLoadAsync();
        await Assertions.Expect(RestaurantCards.First).ToBeVisibleAsync(new() { Timeout = 10000 });
    }
    
    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }

    public async Task<bool> IsNoResultsDisplayedAsync()
    {
        return await NoResultsContainer.IsVisibleAsync();
    }

    public async Task SearchByNameAsync(string name)
    {
        await SearchNameInput.FillAsync(name);
        await SearchButton.ClickAsync();
        await WaitForPageLoadAsync();
    }
    
    public async Task SearchByLocationAsync(string location)
    {
        await SearchLocationInput.FillAsync(location);
        await SearchButton.ClickAsync();
        await WaitForPageLoadAsync();
    }
    
    public async Task SearchAsync(string name, string location)
    {
        await SearchNameInput.FillAsync(name);
        await SearchLocationInput.FillAsync(location);
        await SearchButton.ClickAsync();
        await WaitForPageLoadAsync();
    }


    public async Task ClearFiltersAsync()
    {
        await ClearFiltersButton.ClickAsync();
        await WaitForPageLoadAsync();
    }

    public async Task<int> GetRestaurantCountAsync()
    {
        return await RestaurantCards.CountAsync();
    }

    public async Task<string> GetResultsInfoTextAsync()
    {
        return await ResultsInfo.InnerTextAsync();
    }
    
    public async Task<List<string>> GetAllRestaurantNamesAsync()
    {
        var names = new List<string>();
        var count = await RestaurantNames.CountAsync();

        for (int i = 0; i < count; i++)
        {
            names.Add(await RestaurantNames.Nth(i).InnerTextAsync());
        }

        return names;
    }

    public async Task<List<string>> GetAllRestaurantAddressesAsync()
    {
        var addresses = new List<string>();
        var count = await RestaurantAddresses.CountAsync();

        for (int i = 0; i < count; i++)
        {
            addresses.Add(await RestaurantAddresses.Nth(i).InnerTextAsync());
        }

        return addresses;
    }

    public async Task ClickRestaurantCardAsync(int index)
    {
        await RestaurantCards.Nth(index).ClickAsync();
    }

    public async Task ClickRestaurantByNameAsync(string restaurantName)
    {
        var card = _page.Locator($".restaurant-card:has(.restaurant-name:has-text('{restaurantName}'))");
        await card.ClickAsync();
    }

    public async Task ClickBookTableAsync(int index)
    {
        var bookButton = RestaurantCards.Nth(index).Locator(".btn-book");
        await bookButton.ClickAsync();
    }

    public async Task ClickBookTableByNameAsync(string restaurantName)
    {
        var card = _page.Locator($".restaurant-card:has(.restaurant-name:has-text('{restaurantName}'))");
        var bookButton = card.Locator(".btn-book");
        await bookButton.ClickAsync();
    }
    
    public async Task ClickViewDetailsAsync(int index)
    {
        var viewButton = RestaurantCards.Nth(index).Locator(".btn-view");
        await viewButton.ClickAsync();
    }

    public async Task SortByAsync(string sortOption)
    {
        await SortDropdownButton.ClickAsync();

        var optionLocator = sortOption.ToLower() switch
        {
            "az" or "a-z" => SortOptionAZ,
            "za" or "z-a" => SortOptionZA,
            "highest" or "best" => SortOptionHighestRating,
            "lowest" or "worst" => SortOptionLowestRating,
            _ => throw new ArgumentException($"Unknown sort option: {sortOption}")
        };

        await optionLocator.ClickAsync();
        await WaitForPageLoadAsync();
    }
    
    public async Task SetPageSizeAsync(int pageSize)
    {
        await PageSizeDropdownButton.ClickAsync();

        var optionLocator = pageSize switch
        {
            10 => PageSizeOption10,
            25 => PageSizeOption25,
            50 => PageSizeOption50,
            _ => throw new ArgumentException($"Invalid page size: {pageSize}. Valid options: 10, 25, 50")
        };

        await optionLocator.ClickAsync();
        await WaitForPageLoadAsync();
    }

    public async Task LoadMoreRestaurantsAsync()
    {
        await LoadMoreButton.ClickAsync();
        await Assertions.Expect(LoadMoreSpinner).Not.ToBeVisibleAsync(new() { Timeout = 10000 });
    }

    public async Task<bool> HasMoreRestaurantsAsync()
    {
        return await LoadMoreButton.IsVisibleAsync();
    }

    public async Task<bool> AreAllRestaurantsLoadedAsync()
    {
        return await AllLoadedMessage.IsVisibleAsync();
    }

    public async Task<string> GetSearchNameValueAsync()
    {
        return await SearchNameInput.InputValueAsync();
    }
    
    public async Task<string> GetSearchLocationValueAsync()
    {
        return await SearchLocationInput.InputValueAsync();
    }
    
    public async Task<bool> IsRestaurantDisplayedAsync(string restaurantName)
    {
        var card = _page.Locator($".restaurant-card:has(.restaurant-name:has-text('{restaurantName}'))");
        return await card.IsVisibleAsync();
    }
    
    public async Task<RestaurantCardInfo> GetRestaurantCardInfoAsync(int index)
    {
        var card = RestaurantCards.Nth(index);

        var name = await card.Locator(".restaurant-name").InnerTextAsync();
        var address = await card.Locator(".detail-item:has(.bi-geo-alt-fill) span").InnerTextAsync();

        string? openingStatus = null;
        var hoursElement = card.Locator(".detail-item:has(.bi-clock) span");
        if (await hoursElement.IsVisibleAsync())
        {
            openingStatus = await hoursElement.InnerTextAsync();
        }

        var hasMenu = await card.Locator(".detail-item:has-text('Menu available')").IsVisibleAsync();

        return new RestaurantCardInfo
        {
            Name = name,
            Address = address,
            OpeningStatus = openingStatus,
            HasMenu = hasMenu
        };
    }

    public async Task WaitForRestaurantDetailNavigationAsync()
    {
        await _page.WaitForURLAsync("**/restaurant/**");
    }

    public async Task WaitForBookingNavigationAsync()
    {
        await _page.WaitForURLAsync("**/booking/table/**");
    }

    public async Task AssertPageTitleVisibleAsync()
    {
        await Assertions.Expect(PageTitle).ToBeVisibleAsync();
        await Assertions.Expect(PageTitle).ToHaveTextAsync("Restaurants");
    }

    public async Task AssertNoResultsDisplayedAsync()
    {
        await Assertions.Expect(NoResultsContainer).ToBeVisibleAsync();
        await Assertions.Expect(NoResultsTitle).ToHaveTextAsync("No restaurants found");
    }
    
    public async Task AssertRestaurantCountAsync(int expectedCount)
    {
        await Assertions.Expect(RestaurantCards).ToHaveCountAsync(expectedCount);
    }
}

public class RestaurantCardInfo
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? OpeningStatus { get; set; }
    public bool HasMenu { get; set; }
}