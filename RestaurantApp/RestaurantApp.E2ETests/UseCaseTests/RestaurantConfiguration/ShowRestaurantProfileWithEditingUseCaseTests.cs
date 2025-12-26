using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class ShowRestaurantProfileWithEditingUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
    }

    [Test]
    public async Task VerifiedUser_CanAccessEditPage()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();

        // Act
        await _editPage.NavigateAsync(TestData.RestaurantId);
        await WaitForBlazorAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex($".*/restaurant/edit/{TestData.RestaurantId}"));
    }

    [Test]
    public async Task VerifiedUser_CanSeeAllAvailableTabs()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();

        // Act
        await _editPage.NavigateAsync(TestData.RestaurantId);
        await WaitForBlazorAsync();

        // Assert
        var visibleTabs = await _editPage.GetVisibleTabNamesAsync();

        // Basic Info is always visible
        Assert.That(visibleTabs, Does.Contain("Basic Info"));
        
        // At least one tab should be visible
        Assert.That(visibleTabs.Count, Is.GreaterThanOrEqualTo(6));
    }

    [Test]
    public async Task VerifiedUser_CanSwitchBetweenAllVisibleTabs()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(TestData.RestaurantId);
        await WaitForBlazorAsync();

        // Get all visible tabs
        var visibleTabs = await _editPage.GetVisibleTabNamesAsync();

        // Act & Assert - switch to each tab and verify it becomes active
        foreach (var tabName in visibleTabs)
        {
            await SwitchToTabByNameAsync(tabName);
            await WaitForBlazorAsync();

            var activeTab = await _editPage.GetActiveTabNameAsync();
            Assert.That(activeTab.Trim(), Does.Contain(tabName.Trim()), 
                $"Tab '{tabName}' should be active after clicking it");
        }
    }

    [Test]
    public async Task VerifiedUser_BasicInfoTabIsDefaultActive()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();

        // Act
        await _editPage.NavigateAsync(TestData.RestaurantId);
        await WaitForBlazorAsync();

        // Assert
        var activeTab = await _editPage.GetActiveTabNameAsync();
        Assert.That(activeTab, Does.Contain("Basic Info"));
    }

    [Test]
    public async Task VerifiedUser_TabsLoadCorrectContent()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(TestData.RestaurantId);
        await WaitForBlazorAsync();

        var visibleTabs = await _editPage.GetVisibleTabNamesAsync();

        // Assert each visible tab loads its content
        foreach (var tabName in visibleTabs)
        {
            await SwitchToTabByNameAsync(tabName);
            await WaitForBlazorAsync();

            var contentVisible = await VerifyTabContentAsync(tabName);
            Assert.That(contentVisible, Is.True, 
                $"Content for tab '{tabName}' should be visible");
        }
    }

    [Test]
    public async Task UnauthenticatedUser_CannotAccessEditPage()
    {
        // Act - try to navigate without login
        await Page.GotoAsync($"/restaurant/edit/{TestData.RestaurantId}");
        await WaitForBlazorAsync();

        // Assert - should be redirected to login or see unauthorized
        var currentUrl = Page.Url;
        var isOnLoginPage = currentUrl.Contains("/login");
        var hasUnauthorizedMessage = await Page.Locator("text=unauthorized, text=login, text=sign in").IsVisibleAsync();

        Assert.That(isOnLoginPage || hasUnauthorizedMessage, Is.True,
            "Unauthenticated user should be redirected to login or see unauthorized message");
    }

    // Helper methods
    private async Task SwitchToTabByNameAsync(string tabName)
    {
        var cleanName = tabName.Trim();
        
        if (cleanName.Contains("Basic"))
            await _editPage.SwitchToBasicInfoAsync();
        else if (cleanName.Contains("Tables"))
            await _editPage.SwitchToTablesAsync();
        else if (cleanName.Contains("Settings"))
            await _editPage.SwitchToSettingsAsync();
        else if (cleanName.Contains("Employees"))
            await _editPage.SwitchToEmployeesAsync();
        else if (cleanName.Contains("Menu"))
            await _editPage.SwitchToMenuAsync();
        else if (cleanName.Contains("Appearance"))
            await _editPage.SwitchToAppearanceAsync();
    }

    private async Task<bool> VerifyTabContentAsync(string tabName)
    {
        var cleanName = tabName.Trim();

        return cleanName switch
        {
            var n when n.Contains("Basic") => 
                await Page.Locator("h3:has-text('Change basic information')").IsVisibleAsync(),
            
            var n when n.Contains("Tables") => 
                await Page.Locator("h3:has-text('Configure tables')").IsVisibleAsync(),
            
            var n when n.Contains("Settings") => 
                await Page.Locator("h3:has-text('Restaurant Settings'), .card:has-text('Reservation')").First.IsVisibleAsync(),
            
            var n when n.Contains("Employees") => 
                await Page.Locator("button:has-text('Add new employee')").IsVisibleAsync(),
            
            var n when n.Contains("Menu") => 
                await Page.Locator("h3:has-text('Manage restaurant menu'), #menuName").First.IsVisibleAsync(),
            
            var n when n.Contains("Appearance") => 
                await Page.Locator("text=Profile Photo").IsVisibleAsync(),
            
            _ => true // Unknown tab - assume content is loaded
        };
    }
    
    private static class TestData
    {
        public const int RestaurantId = 1;
    }
}