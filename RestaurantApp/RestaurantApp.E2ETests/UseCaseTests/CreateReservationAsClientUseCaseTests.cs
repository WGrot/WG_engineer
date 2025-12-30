using System.Text.RegularExpressions;
using Microsoft.Playwright;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests;

[TestFixture]
public class CreateReservationAsClientUseCaseTests : PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantsListPage _restaurantsListPage = null!;
    private RestaurantDetailsPage _restaurantDetailsPage = null!;

    private int _testRestaurantId;
    private string _testRestaurantName = string.Empty;

    private readonly TimeOnly _openingTime = new(10, 0);
    private readonly TimeOnly _closingTime = new(22, 0);

    [SetUp]
    public async Task SetUp()
    {
        _loginPage = new LoginPage(Page);
        _restaurantsListPage = new RestaurantsListPage(Page);
        _restaurantDetailsPage = new RestaurantDetailsPage(Page);

        var credentials = TestDataFactory.GetTestUserCredentials(4);
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await WaitForBlazorAsync();

        await FindRestaurantWithTablesAsync();
    }

    private async Task FindRestaurantWithTablesAsync()
    {
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        var restaurantCount = await _restaurantsListPage.GetRestaurantCountAsync();
        if (restaurantCount == 0)
        {
            Assert.Ignore("No restaurants available for testing");
            return;
        }

        for (int i = 0; i < Math.Min(restaurantCount, 5); i++)
        {
            await _restaurantsListPage.GotoAsync();
            await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

            var names = await _restaurantsListPage.GetAllRestaurantNamesAsync();
            if (i < names.Count)
            {
                _testRestaurantName = names[i];
            }

            await _restaurantsListPage.ClickRestaurantCardAsync(i);
            await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();

            var url = Page.Url;
            var match = Regex.Match(url, @"/restaurant/(\d+)");
            if (!match.Success) continue;

            _testRestaurantId = int.Parse(match.Groups[1].Value);


            await _restaurantDetailsPage.WaitForPageLoadAsync();
            await _restaurantDetailsPage.SwitchToTablesTabAsync();
            await WaitForBlazorAsync();

            var hasTables = await _restaurantDetailsPage.Tables.AreTablesDisplayedAsync();
            if (hasTables)
            {
                return;
            }
        }

        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();
        await _restaurantsListPage.ClickRestaurantCardAsync(0);
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();

        var firstUrl = Page.Url;
        var firstMatch = Regex.Match(firstUrl, @"/restaurant/(\d+)");
        if (firstMatch.Success)
        {
            _testRestaurantId = int.Parse(firstMatch.Groups[1].Value);
        }
    }

    private async Task NavigateToTableBookingTabAsync()
    {
        await _restaurantDetailsPage.GotoAsync(_testRestaurantId);
        await _restaurantDetailsPage.SwitchToTableBookingTabAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.TableBooking.WaitForFormLoadAsync();
    }

    private DateTime GetTomorrowDate() => DateTime.Now.Date.AddDays(1);

    private (TimeOnly start, TimeOnly end) GetValidReservationTime()
    {
        return (new TimeOnly(12, 0), new TimeOnly(14, 0));
    }


    [Test]
    [Description("Verify clicking next day advances the date")]
    public async Task ClickNextDay_AdvancesDate()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var initialDate = await _restaurantDetailsPage.TableBooking.GetSelectedDateAsync();

        // Act
        await _restaurantDetailsPage.TableBooking.ClickNextDayAsync();

        // Assert
        var newDate = await _restaurantDetailsPage.TableBooking.GetSelectedDateAsync();
        Assert.That(newDate.Date, Is.EqualTo(initialDate.Date.AddDays(1)),
            "Date should advance by one day");
    }

    [Test]
    [Description("Verify clicking previous day goes back")]
    public async Task ClickPreviousDay_GoesBack()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        await _restaurantDetailsPage.TableBooking.ClickNextDayAsync(); // Go forward first
        var initialDate = await _restaurantDetailsPage.TableBooking.GetSelectedDateAsync();

        // Act
        await _restaurantDetailsPage.TableBooking.ClickPreviousDayAsync();

        // Assert
        var newDate = await _restaurantDetailsPage.TableBooking.GetSelectedDateAsync();
        Assert.That(newDate.Date, Is.EqualTo(initialDate.Date.AddDays(-1)),
            "Date should go back by one day");
    }

    [Test]
    [Description("Verify selecting specific date works")]
    public async Task SelectDate_Tomorrow_UpdatesDate()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();

        // Act
        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);

        // Assert
        var selectedDate = await _restaurantDetailsPage.TableBooking.GetSelectedDateAsync();
        Assert.That(selectedDate.Date, Is.EqualTo(tomorrow.Date),
            "Selected date should be tomorrow");
    }


    [Test]
    [Description("Verify checking availability shows available tables")]
    public async Task CheckAvailability_ValidTimeSlot_ShowsAvailableTables()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();
        var (startTime, endTime) = GetValidReservationTime();

        // Act
        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(startTime);
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(endTime);
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await _restaurantDetailsPage.TableBooking.WaitForAvailabilityCheckAsync();

        // Assert
        var isAvailableSectionVisible =
            await _restaurantDetailsPage.TableBooking.IsAvailableTablesSectionVisibleAsync();
        Assert.That(isAvailableSectionVisible, Is.True,
            "Available tables section should be displayed after checking availability");
    }

    [Test]
    [Description("Verify checking availability with invalid time range shows error")]
    public async Task CheckAvailability_EndTimeBeforeStartTime_ShowsError()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();

        // Act - set end time before start time
        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(new TimeOnly(14, 0));
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(new TimeOnly(12, 0)); // Before start!
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await WaitForBlazorAsync();

        // Assert - should show error toast
        var isErrorVisible = await _restaurantDetailsPage.TableBooking.IsErrorToastVisibleAsync();
        Assert.That(isErrorVisible, Is.True,
            "Error toast should be displayed when end time is before start time");
    }

    [Test]
    public async Task CheckAvailability_OutsideOpeningHours_ShowsError()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();

        // Act - set time outside opening hours (before 10:00)
        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(new TimeOnly(8, 0));
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(new TimeOnly(9, 0));
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await WaitForBlazorAsync();

        // Assert - should show error or no tables
        var isErrorVisible = await _restaurantDetailsPage.TableBooking.IsErrorToastVisibleAsync();
        var noTables = await _restaurantDetailsPage.TableBooking.IsNoTablesAvailableAsync();

        Assert.That(isErrorVisible || noTables, Is.True,
            "Should show error or no tables when outside opening hours");
    }


    [Test]
    public async Task SelectTable_ShowsCustomerInfoForm()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();
        var (startTime, endTime) = GetValidReservationTime();

        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(startTime);
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(endTime);
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await _restaurantDetailsPage.TableBooking.WaitForAvailabilityCheckAsync();

        var tableCount = await _restaurantDetailsPage.TableBooking.GetAvailableTableCountAsync();
        if (tableCount == 0)
        {
            Assert.Ignore("No tables available for selected time slot");
            return;
        }

        // Act
        await _restaurantDetailsPage.TableBooking.SelectFirstAvailableTableAsync();
        await WaitForBlazorAsync();

        // Assert
        var isCustomerFormVisible = await _restaurantDetailsPage.TableBooking.IsCustomerInfoFormVisibleAsync();
        Assert.That(isCustomerFormVisible, Is.True,
            "Customer info form should be displayed after selecting a table");
    }

    [Test]
    public async Task SelectTable_LoggedInUser_CustomerInfoIsPrefilled()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();
        var (startTime, endTime) = GetValidReservationTime();

        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(startTime);
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(endTime);
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await _restaurantDetailsPage.TableBooking.WaitForAvailabilityCheckAsync();

        var tableCount = await _restaurantDetailsPage.TableBooking.GetAvailableTableCountAsync();
        if (tableCount == 0)
        {
            Assert.Ignore("No tables available for selected time slot");
            return;
        }

        // Act
        await _restaurantDetailsPage.TableBooking.SelectFirstAvailableTableAsync();
        await WaitForBlazorAsync();

        // Assert - logged-in user should have pre-filled data
        var isPrefilled = await _restaurantDetailsPage.TableBooking.IsCustomerInfoPrefilledAsync();
        Assert.That(isPrefilled, Is.True,
            "Customer info should be pre-filled for logged-in user");
    }


    [Test]
    public async Task SubmitReservation_ValidData_ShowsSuccessToast()
    {
        // Arrange
        await NavigateToTableBookingTabAsync();
        var tomorrow = GetTomorrowDate();
        var (startTime, endTime) = GetValidReservationTime();

        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(startTime);
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(endTime);
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await _restaurantDetailsPage.TableBooking.WaitForAvailabilityCheckAsync();

        var tableCount = await _restaurantDetailsPage.TableBooking.GetAvailableTableCountAsync();
        if (tableCount == 0)
        {
            Assert.Ignore("No tables available for selected time slot");
            return;
        }

        await _restaurantDetailsPage.TableBooking.SelectFirstAvailableTableAsync();
        await WaitForBlazorAsync();

        await _restaurantDetailsPage.TableBooking.FillCustomerInfoAsync(
            "Test Customer",
            "+48123456789",
            "test@example.com"
        );
        

        await _restaurantDetailsPage.TableBooking.ClickConfirmReservationAsync();

        await WaitForBlazorAsync();

        // Assert
        var isSuccess = await _restaurantDetailsPage.TableBooking.IsSuccessToastVisibleAsync();
        Assert.That(isSuccess, Is.True, "Success toast should be displayed after successful reservation");
    }


    [Test]
    public async Task CompleteReservationFlow_FromRestaurantList_ToConfirmation()
    {
        // Arrange - Start from restaurant list
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Act - Navigate to restaurant
        await _restaurantsListPage.ClickRestaurantCardAsync(0);
        await _restaurantsListPage.WaitForRestaurantDetailNavigationAsync();
        await _restaurantDetailsPage.WaitForPageLoadAsync();

        // Click Book Table in header
        await _restaurantDetailsPage.ClickBookTableInHeaderAsync();
        await WaitForBlazorAsync();
        await _restaurantDetailsPage.TableBooking.WaitForFormLoadAsync();

        // Set reservation details
        var tomorrow = GetTomorrowDate();
        var (startTime, endTime) = GetValidReservationTime();

        await _restaurantDetailsPage.TableBooking.SelectDateAsync(tomorrow);
        await _restaurantDetailsPage.TableBooking.SetStartTimeAsync(startTime);
        await _restaurantDetailsPage.TableBooking.SetEndTimeAsync(endTime);
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(4);

        // Check availability
        await _restaurantDetailsPage.TableBooking.ClickCheckAvailabilityAsync();
        await _restaurantDetailsPage.TableBooking.WaitForAvailabilityCheckAsync();

        var tableCount = await _restaurantDetailsPage.TableBooking.GetAvailableTableCountAsync();
        if (tableCount == 0)
        {
            Assert.Ignore("No tables available for selected time slot");
            return;
        }

        // Select table
        await _restaurantDetailsPage.TableBooking.SelectFirstAvailableTableAsync();
        await WaitForBlazorAsync();


        await _restaurantDetailsPage.TableBooking.FillCustomerInfoAsync(
            "Complete Flow Test",
            "+48111222333",
            "flow.test@example.com",
            "Testing complete reservation flow"
        );
        
        
        
        // Confirm reservation
        await _restaurantDetailsPage.TableBooking.ClickConfirmReservationAsync();
        await WaitForBlazorAsync();

        await _restaurantDetailsPage.TableBooking.WaitForSuccessToastAsync();
        // Assert
        var isSuccess = await _restaurantDetailsPage.TableBooking.IsSuccessToastVisibleAsync();
        Assert.That(isSuccess, Is.True, "Complete reservation flow should succeed");
    }
}