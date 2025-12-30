using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.UserSettings;

/// <summary>
/// Main Page Object for the User Settings page.
/// Handles navigation between tabs and provides access to tab-specific page objects.
/// </summary>
public class UserSettingsPage
{
    private readonly IPage _page;

    // Header selectors - multiple options for robustness
    private const string PageHeader = ".restaurant-header, .bg-primary.text-white";
    private const string UserNameHeader = ".restaurant-header h1, .bg-primary h1";
    private const string UserEmailText = ".restaurant-header p:has(.bi-envelope), p:has(.bi-envelope)";
    private const string UserPhoneText = ".restaurant-header p:has(.bi-telephone), p:has(.bi-telephone)";

    // Tab button selectors
    private const string InfoTabButton = "button:has(.bi-info-circle)";
    private const string SecurityTabButton = "button:has(.bi-shield-lock)";
    private const string EmployeeTabButton = "button:has(.bi-person-badge)";
    
    // Alternative tab selectors
    private const string NavTabs = ".nav-tabs, ul.nav";

    public UserSettingsPage(IPage page)
    {
        _page = page;
    }

    /// <summary>
    /// Navigates to the User Settings page.
    /// </summary>
    public async Task NavigateAsync()
    {
        await _page.GotoAsync("/UserSettings");
        await _page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
    }

    /// <summary>
    /// Waits for the page to fully load.
    /// </summary>
    public async Task WaitForPageLoadAsync()
    {
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Wait for any of the main content indicators
        await _page.WaitForSelectorAsync(".restaurant-header, .bg-primary, .nav-tabs", 
            new() { Timeout = 15000 });
    }

    // ==================== Header Information ====================

    /// <summary>
    /// Gets the displayed user's full name from the header.
    /// </summary>
    public async Task<string> GetUserNameAsync()
    {
        var element = _page.Locator(UserNameHeader);
        return await element.TextContentAsync() ?? string.Empty;
    }

    /// <summary>
    /// Gets the displayed user's email from the header.
    /// </summary>
    public async Task<string> GetUserEmailAsync()
    {
        var element = _page.Locator(UserEmailText);
        var text = await element.TextContentAsync() ?? string.Empty;
        return text.Trim();
    }

    /// <summary>
    /// Gets the displayed user's phone number from the header (if present).
    /// </summary>
    public async Task<string?> GetUserPhoneAsync()
    {
        var element = _page.Locator(UserPhoneText);
        if (await element.CountAsync() == 0)
            return null;

        var text = await element.TextContentAsync() ?? string.Empty;
        return text.Trim();
    }

    // ==================== Tab Navigation ====================

    /// <summary>
    /// Clicks the Info tab and returns the UserInfoTab page object.
    /// </summary>
    public async Task<UserInfoTab> GoToInfoTabAsync()
    {
        await _page.Locator(InfoTabButton).ClickAsync();
        await _page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        // Wait for the User Information card to be visible (now in view mode by default)
        await _page.WaitForSelectorAsync(".card:has-text('User Information')");
        return new UserInfoTab(_page);
    }

    /// <summary>
    /// Clicks the Security tab and returns the SecuritySettingsTab page object.
    /// </summary>
    public async Task<SecuritySettingsTab> GoToSecurityTabAsync()
    {
        await _page.Locator(SecurityTabButton).ClickAsync();
        await _page.WaitForSelectorAsync(".card:has(.bi-key)");
        return new SecuritySettingsTab(_page);
    }

    /// <summary>
    /// Clicks the Employee Settings tab and returns the EmployeeSettingsTab page object.
    /// </summary>
    public async Task<EmployeeSettingsTab> GoToEmployeeTabAsync()
    {
        var employeeTab = _page.Locator(EmployeeTabButton);
        if (await employeeTab.CountAsync() == 0)
            throw new InvalidOperationException("Employee tab is not visible. User may not be an employee.");

        await employeeTab.ClickAsync();
        await _page.WaitForSelectorAsync("h3:has-text('User employment')");
        return new EmployeeSettingsTab(_page);
    }

    /// <summary>
    /// Checks if the Employee Settings tab is visible (only shown for employees).
    /// </summary>
    public async Task<bool> IsEmployeeTabVisibleAsync()
    {
        return await _page.Locator(EmployeeTabButton).CountAsync() > 0;
    }

    /// <summary>
    /// Gets the currently active tab name.
    /// </summary>
    public async Task<string> GetActiveTabNameAsync()
    {
        var activeTab = _page.Locator(".nav-link.active");
        var text = await activeTab.TextContentAsync() ?? string.Empty;
        return text.Trim();
    }

    /// <summary>
    /// Checks if a specific tab is currently active.
    /// </summary>
    public async Task<bool> IsTabActiveAsync(TabType tab)
    {
        var selector = tab switch
        {
            TabType.Info => InfoTabButton,
            TabType.Security => SecurityTabButton,
            TabType.Employee => EmployeeTabButton,
            _ => throw new ArgumentOutOfRangeException(nameof(tab))
        };

        var tabElement = _page.Locator(selector);
        var classes = await tabElement.GetAttributeAsync("class") ?? string.Empty;
        return classes.Contains("active");
    }
}

public enum TabType
{
    Info,
    Security,
    Employee
}