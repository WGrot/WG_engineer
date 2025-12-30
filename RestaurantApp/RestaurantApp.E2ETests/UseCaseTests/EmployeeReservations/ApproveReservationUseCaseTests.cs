using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;
[TestFixture]
public class ApproveReservationUseCaseTests: PlaywrightTestBase
    {
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _loginPage = new LoginPage(Page);
        _dashboardPage = new RestaurantDashboardPage(Page);

        var credentials = TestDataFactory.GetMultiRestaurantEmployeeCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task ApproveReservation_RemovesFromPendingSectionAndShowsSuccessToast()
    {
        // Arrange
        await _dashboardPage.PendingReservations.WaitForLoadAsync();
        await WaitForBlazorAsync();

        var initialCount = await _dashboardPage.PendingReservations.GetReservationCountAsync();
    
        if (initialCount == 0)
        {
            Assert.Ignore("No pending reservations available for testing");
            return;
        }

        var reservationToApprove = await _dashboardPage.PendingReservations.GetReservationDetailsAsync(0);

        // Act
        await _dashboardPage.PendingReservations.ApproveReservationAsync(0);
        await WaitForBlazorAsync();

        // Assert
        await _dashboardPage.WaitForSuccessToastAsync();
    
        var remainingReservations = await _dashboardPage.PendingReservations.GetAllReservationsAsync();
    
        var isReservationStillPresent = remainingReservations.Any(r => 
            r.CustomerName == reservationToApprove.CustomerName &&
            r.CustomerEmail == reservationToApprove.CustomerEmail &&
            r.Date == reservationToApprove.Date &&
            r.Time == reservationToApprove.Time);
    
        Assert.Multiple(async () =>
        {
            Assert.That(isReservationStillPresent, Is.False,
                $"Reservation for '{reservationToApprove.CustomerName}' on {reservationToApprove.Date} at {reservationToApprove.Time} should no longer be in the pending list after approval");
            Assert.That(await _dashboardPage.IsSuccessToastVisibleAsync(), Is.True,
                "Success toast should be visible after approving reservation");
        });
    }
}