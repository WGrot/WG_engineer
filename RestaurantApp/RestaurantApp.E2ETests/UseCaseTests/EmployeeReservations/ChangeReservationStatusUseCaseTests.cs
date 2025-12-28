using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;


[TestFixture]
public class ChangeReservationStatusUseCaseTests: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;
    private string _testCustomerName = null!;

    [SetUp]
    public async Task Setup()
    {
        _loginPage = new LoginPage(Page);
        _dashboardPage = new RestaurantDashboardPage(Page);

        var credentials = TestDataFactory.GetValidUserCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Create a reservation for today at end of day
        await CreateTodayReservationAsync();

        // Refresh page
        await _dashboardPage.NavigateAsync();
        await _dashboardPage.UpcomingReservations.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    private async Task CreateTodayReservationAsync()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _testCustomerName = $"Status Test {uniqueId}";

        await _dashboardPage.OpenNewReservationModalAsync();

        // Set today's date (should be default)
        await _dashboardPage.NewReservationModal.SetStartTimeAsync("21:00");
        await _dashboardPage.NewReservationModal.SetEndTimeAsync("22:00");
        await _dashboardPage.NewReservationModal.SetGuestsAsync(2);
        await _dashboardPage.NewReservationModal.CheckAvailabilityAsync();
        await WaitForBlazorAsync();

        if (!await _dashboardPage.NewReservationModal.HasAvailableTablesAsync())
        {
            await _dashboardPage.NewReservationModal.CloseAsync();
            Assert.Ignore("No tables available for testing");
            return;
        }

        await _dashboardPage.NewReservationModal.SelectTableByIndexAsync(0);
        await WaitForBlazorAsync();

        await _dashboardPage.NewReservationModal.FillCustomerInfoAsync(new CustomerInfoData
        {
            Name = _testCustomerName,
            Email = $"status.test.{uniqueId}@example.com",
            Phone = "123456789"
        });

        await _dashboardPage.NewReservationModal.ConfirmReservationAndWaitAsync();
        await _dashboardPage.WaitForSuccessToastAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task ChangeStatusToCompleted_RemovesFromUpcomingReservations()
    {
        // Arrange
        var initialCount = await _dashboardPage.UpcomingReservations.GetReservationCountAsync();

        if (initialCount == 0)
        {
            Assert.Ignore("No upcoming reservations available for testing");
            return;
        }

        // Act - open first reservation details
        await _dashboardPage.UpcomingReservations.ClickReservationAsync(0);
        await WaitForBlazorAsync();

        // Change status to Completed
        await _dashboardPage.UpcomingReservations.DetailsModal.UpdateStatusAsync("Completed");
        await WaitForBlazorAsync();
        
        // Close modal
        await _dashboardPage.UpcomingReservations.DetailsModal.CloseAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _dashboardPage.UpcomingReservations.GetReservationCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount - 1),
            "Upcoming reservations count should decrease by 1 after changing status to Completed");
    }
}