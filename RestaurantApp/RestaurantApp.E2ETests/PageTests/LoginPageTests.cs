using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.PageTests;

[TestFixture]
public class LoginPageTests : PlaywrightTestBase
{
    private LoginPage _loginPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        _loginPage = new LoginPage(Page);
        await _loginPage.GotoAsync();
    }
    
    [Test]
    public async Task Login_ClickRegisterLink_ShouldNavigateToRegisterPage()
    {
        // Act
        await _loginPage.RegisterLink.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/register"));
    }
    
    [Test]
    public async Task Login_ClickForgotPasswordLink_ShouldNavigateToForgotPasswordPage()
    {
        // Act
        await _loginPage.ForgotPasswordLink.ClickAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/forgot-password"));
    }
}