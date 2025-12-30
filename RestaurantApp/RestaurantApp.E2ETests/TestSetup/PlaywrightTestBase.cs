using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using RestaurantApp.E2ETests.PageObjects;

namespace RestaurantApp.E2ETests.TestSetup;

public class PlaywrightTestBase: PageTest
{
    protected LoginPage _loginPage = null!;
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            BaseURL = TestConfiguration.BaseUrl,
            IgnoreHTTPSErrors = true,
            Locale = "en-US",
            TimezoneId = "Europe/Warsaw",
            ViewportSize = new ViewportSize
            {
                Width = 1920,
                Height = 1080
            },
            RecordVideoDir = TestConfiguration.RecordVideo ? "videos/" : null
        };
    }

    [SetUp]
    public async Task BaseSetUp()
    {
        Page.SetDefaultTimeout(TestConfiguration.DefaultTimeout);
        _loginPage = new LoginPage(Page);
        
        Page.SetDefaultNavigationTimeout(TestConfiguration.NavigationTimeout);
    }

    [TearDown]
    public async Task BaseTearDown()
    {
        if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            var screenshotPath = Path.Combine(
                "screenshots",
                $"{TestContext.CurrentContext.Test.Name}_{DateTime.Now:yyyyMMdd_HHmmss}.png"
            );
            
            Directory.CreateDirectory("screenshots");
            await Page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = true });
            
            TestContext.AddTestAttachment(screenshotPath, "Screenshot on failure");
        }
    }
    
    protected async Task WaitForBlazorAsync()
    {
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        await Page.WaitForTimeoutAsync(100);
    }
    
    protected async Task<bool> IsUserAuthenticatedAsync()
    {
        var logoutButton = Page.Locator("[data-testid='logout-button'], .logout-btn, a[href='/logout']");
        return await logoutButton.IsVisibleAsync();
    }
    
    public async Task LoginAsVerifiedUserAsync()
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync("jan@kowalski.com", "123123123");
        
        
        await WaitForBlazorAsync();
        await Page.WaitForTimeoutAsync(500);
    }
    
    public async Task LoginAsUserAsync(string name, string password)
    {
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(name, password);
        
        
        await WaitForBlazorAsync();
        await Page.WaitForTimeoutAsync(500);
    }
    
}