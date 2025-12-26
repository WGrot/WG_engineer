using System.Text.RegularExpressions;
using Microsoft.Playwright;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class CreateRestaurantUseCaseTest : PlaywrightTestBase
{
    private CreateRestaurantPage _createRestaurantPage = null!;
    
    [SetUp]
    public async Task SetUp()
    {
        _createRestaurantPage = new CreateRestaurantPage(Page);
    }



    #region Authorization Tests

    [Test]
    public async Task CreateRestaurant_WhenNotAuthenticated_ShouldShowNotAuthorizedMessage()
    {
        // Act
        await _createRestaurantPage.GotoAsync();

        // Assert
        await Expect(_createRestaurantPage.NotAuthorizedMessage).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.LoginButton).ToBeVisibleAsync();
    }

    [Test]
    public async Task CreateRestaurant_WhenNotAuthenticated_ClickLogin_ShouldRedirectToLoginWithReturnUrl()
    {
        // Arrange
        await _createRestaurantPage.GotoAsync();

        // Act
        await _createRestaurantPage.ClickLoginButtonAsync();

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/login\?returnUrl=.*CreateRestaurant"));
    }

    [Test]
    public async Task CreateRestaurant_WhenAuthenticated_ShouldShowForm()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();

        // Act
        await _createRestaurantPage.GotoAsync();

        // Assert - use more flexible locator

        await Expect(_createRestaurantPage.RestaurantNameInput).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.StreetInput).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.CityInput).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.PostalCodeInput).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.CountryInput).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.SubmitButton).ToBeVisibleAsync();
    }

    #endregion
    
    
    
    #region Successful Creation Tests

    [Test]
    public async Task CreateRestaurant_WithValidData_ShouldShowSuccessMessage()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = TestDataFactory.GenerateRestaurant();

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.SuccessToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.SuccessToastBody).ToContainTextAsync("Restaurant created successfully");
    }

    #endregion

    #region Validation Tests - Empty Fields

    [Test]
    public async Task CreateRestaurant_WithEmptyRestaurantName_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = TestDataFactory.GenerateRestaurant();
        testRestaurant.Name = string.Empty;

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.ErrorToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.ErrorToastBody).ToContainTextAsync("Restaurant name is required");
    }

    [Test]
    public async Task CreateRestaurant_WithEmptyStreet_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = TestDataFactory.GenerateRestaurant();
        testRestaurant.Street = string.Empty;

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.ErrorToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.ErrorToastBody).ToContainTextAsync("Street is required");
    }

    [Test]
    public async Task CreateRestaurant_WithEmptyCity_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = TestDataFactory.GenerateRestaurant();
        testRestaurant.City = string.Empty;

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.ErrorToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.ErrorToastBody).ToContainTextAsync("City name is required");
    }

    [Test]
    public async Task CreateRestaurant_WithEmptyPostalCode_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = TestDataFactory.GenerateRestaurant();
        testRestaurant.PostalCode = string.Empty;

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.ErrorToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.ErrorToastBody).ToContainTextAsync("Postal Code is required");
    }

    [Test]
    public async Task CreateRestaurant_WithEmptyCountry_ShouldShowValidationError()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = TestDataFactory.GenerateRestaurant();
        testRestaurant.Country = string.Empty;

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.ErrorToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.ErrorToastBody).ToContainTextAsync("Country name is required");
    }

    #endregion

    #region API Error Tests

    [Test]
    public async Task CreateRestaurant_WithInvalidAddress_ShouldShowGeocodingError()
    {
        // Arrange
        await LoginAsVerifiedUserAsync();
        await _createRestaurantPage.GotoAsync();
        var testRestaurant = new TestRestaurant
        {
            Name = "Test Restaurant",
            Street = "InvalidStreetThatDoesNotExist12345",
            City = "InvalidCity12345",
            PostalCode = "00000",
            Country = "InvalidCountry12345"
        };

        // Act
        await _createRestaurantPage.FillFormAsync(testRestaurant);
        await _createRestaurantPage.SubmitAsync();

        // Assert
        await Expect(_createRestaurantPage.ErrorToast).ToBeVisibleAsync();
        await Expect(_createRestaurantPage.ErrorToastBody).ToContainTextAsync("Geocoding error");
    }

    #endregion
}