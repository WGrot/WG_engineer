using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;

public class RestaurantEditPage
{
    private readonly IPage _page;

    // Tab page objects
    public BasicInfoTab BasicInfo { get; }
    public TablesTab Tables { get; }
    public SettingsTab Settings { get; }
    public EmployeesTab Employees { get; }
    public MenuTab Menu { get; }
    public AppearanceTab Appearance { get; }

    public RestaurantEditPage(IPage page)
    {
        _page = page;
        BasicInfo = new BasicInfoTab(page);
        Tables = new TablesTab(page);
        Settings = new SettingsTab(page);
        Employees = new EmployeesTab(page);
        Menu = new MenuTab(page);
        Appearance = new AppearanceTab(page);
    }

    // Locators
    private ILocator TabButtons => _page.Locator(".nav-tabs .nav-link");
    private ILocator ActiveTabButton => _page.Locator(".nav-tabs .nav-link.active");
    private ILocator LoadingSpinner => _page.Locator(".loading-container .spinner");
    private ILocator EditContainer => _page.Locator(".edit-container");

    // Navigation
    public async Task NavigateAsync(int restaurantId)
    {
        await _page.GotoAsync($"/restaurant/edit/{restaurantId}");
        await WaitForLoadAsync();
    }

    public async Task WaitForLoadAsync()
    {
        await _page.WaitForSelectorAsync(".loading-container", new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await _page.WaitForSelectorAsync(".edit-container");
    }

    // Tab switching
    public async Task SwitchToBasicInfoAsync() => await SwitchTabAsync("Basic Info");
    public async Task SwitchToTablesAsync() => await SwitchTabAsync("Tables");
    public async Task SwitchToSettingsAsync() => await SwitchTabAsync("Settings");
    public async Task SwitchToEmployeesAsync() => await SwitchTabAsync("Employees");
    public async Task SwitchToMenuAsync() => await SwitchTabAsync("Menu");
    public async Task SwitchToAppearanceAsync() => await SwitchTabAsync("Appearance");

    private async Task SwitchTabAsync(string tabName)
    {
        var tabButton = _page.Locator($".nav-tabs .nav-link:has-text('{tabName}')");
        await tabButton.ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    // State checks
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

    public async Task<string> GetActiveTabNameAsync()
    {
        return (await ActiveTabButton.InnerTextAsync()).Trim();
    }

    public async Task<bool> IsTabVisibleAsync(string tabName)
    {
        var tabButton = _page.Locator($".nav-tabs .nav-link:has-text('{tabName}')");
        return await tabButton.IsVisibleAsync();
    }

    public async Task<bool> IsLoadingAsync()
    {
        return await LoadingSpinner.IsVisibleAsync();
    }
}