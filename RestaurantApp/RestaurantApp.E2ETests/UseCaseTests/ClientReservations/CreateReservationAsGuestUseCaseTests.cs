using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.RestaurantDetails;
using RestaurantApp.E2ETests.PageObjects.RestaurantSearch;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.ClientReservations;

[TestFixture]
public class CreateReservationAsGuestUseCaseTests : PlaywrightTestBase
{
    private RestaurantsListPage _restaurantsListPage = null!;
    private RestaurantDetailsPage _restaurantDetailsPage = null!;

    [SetUp]
    public async Task SetUp()
    {
        // NO LOGIN - guest user
        _restaurantsListPage = new RestaurantsListPage(Page);
        _restaurantDetailsPage = new RestaurantDetailsPage(Page);
    }

    private DateTime GetTomorrowDate() => DateTime.Now.Date.AddDays(1);

    private (TimeOnly start, TimeOnly end) GetValidReservationTime()
    {
        return (new TimeOnly(18, 0), new TimeOnly(20, 0));
    }


    [Test]
    public async Task CompleteReservationFlow_WithoutLogin_Succeeds()
    {
        // Arrange - NOT logged in, go directly to restaurant list
        await _restaurantsListPage.GotoAsync();
        await _restaurantsListPage.WaitForRestaurantsLoadedAsync();

        // Navigate to restaurant
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
        await _restaurantDetailsPage.TableBooking.SetGuestCountAsync(2);

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

        // Fill customer info - REQUIRED for non-logged in users
        await _restaurantDetailsPage.TableBooking.FillCustomerInfoAsync(
            "Guest User",
            "+48999888777",
            "guest@example.com",
            "Reservation without account"
        );

        // Confirm reservation
        await _restaurantDetailsPage.TableBooking.ClickConfirmReservationAsync();
        await WaitForBlazorAsync();

        await _restaurantDetailsPage.TableBooking.WaitForSuccessToastAsync();

        // Assert
        var isSuccess = await _restaurantDetailsPage.TableBooking.IsSuccessToastVisibleAsync();
        Assert.That(isSuccess, Is.True, "Reservation without login should succeed");
    }
}