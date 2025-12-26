using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.PageTests;

[TestFixture]
public class RegisterPageTest : PlaywrightTestBase
{
    private RegisterPage _registerPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _registerPage = new RegisterPage(Page);
        await _registerPage.GotoAsync();
    }
    
    [Test]
    public async Task Register_ClickLoginLink_ShouldNavigateToLoginPage()
    {
        // Act
        await _registerPage.LoginLink.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/login"));
    }
}