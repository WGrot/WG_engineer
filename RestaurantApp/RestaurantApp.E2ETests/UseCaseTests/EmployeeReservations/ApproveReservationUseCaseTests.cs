using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;

public class ApproveReservationUseCaseTests: PlaywrightTestBase
    {
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;

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

        // Act
        await _dashboardPage.PendingReservations.ApproveReservationAsync(0);
        await WaitForBlazorAsync();

        // Assert
        await _dashboardPage.WaitForSuccessToastAsync();
        
        var newCount = await _dashboardPage.PendingReservations.GetReservationCountAsync();
        
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount - 1),
                "Pending reservations count should decrease by 1 after approval");
            Assert.That(await _dashboardPage.IsSuccessToastVisibleAsync(), Is.True,
                "Success toast should be visible after approving reservation");
        });
    }
}