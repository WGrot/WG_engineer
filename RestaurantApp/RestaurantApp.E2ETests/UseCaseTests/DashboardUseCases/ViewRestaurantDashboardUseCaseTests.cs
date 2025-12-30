using System.Text.RegularExpressions;
using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.DashboardUseCases;
[TestFixture]
public class ViewRestaurantDashboardUseCaseTests: PlaywrightTestBase
{
    private LoginPage _loginPage = null!;
    private RestaurantDashboardPage _dashboardPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _loginPage = new LoginPage(Page);
        _dashboardPage = new RestaurantDashboardPage(Page);
    }

    [Test]
    public async Task Login_AsEmployee_ShouldDisplayRestaurantDashboard()
    {
        // Arrange
        var credentials = TestDataFactory.GetMultiRestaurantEmployeeCredentials();

        // Act
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);

        // Assert
        await Expect(Page).ToHaveURLAsync(new Regex(@"\/RestaurantDashboard"));
        await _dashboardPage.WaitForLoadAsync();
        Assert.That(await _dashboardPage.IsLoadingAsync(), Is.False,
            "Dashboard should finish loading");
    }

    [Test]
    public async Task Dashboard_ShouldDisplayFourTablesWithCorrectNumbers()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Act
        var tableCount = await _dashboardPage.AvailableTables.GetTableCountAsync();
        var tables = await _dashboardPage.AvailableTables.GetAllTablesAsync();

        // Assert
        Assert.That(tableCount, Is.EqualTo(4), "Should display exactly 4 tables");

        var tableNumbers = tables.Select(t => t.TableNumber).OrderBy(n => n).ToList();
        Assert.That(tableNumbers, Is.EqualTo(new[] { 1, 2, 3, 4 }),
            "Tables should have numbers 1, 2, 3, 4");
    }

    [Test]
    public async Task Dashboard_ShouldDisplayPendingReservations()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await _dashboardPage.PendingReservations.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Act
        var reservationCount = await _dashboardPage.PendingReservations.GetReservationCountAsync();
        var reservations = await _dashboardPage.PendingReservations.GetAllReservationsAsync();

        // Assert
        Assert.That(reservationCount, Is.GreaterThan(0), "Should display exactly 1 pending reservation");
        Assert.That(reservations.First().Status, Is.EqualTo("Pending"),
            "Reservation should have Pending status");
    }

    [Test]
    public async Task Dashboard_ShouldDisplayCorrectStatistics()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await _dashboardPage.AvailableTables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Act
        var tables = await _dashboardPage.AvailableTables.GetAllTablesAsync();
        var stats = await _dashboardPage.GetAllStatisticsAsync();

        // Calculate expected values from tables
        var expectedAvailableTables = tables.Count(t => t.IsAvailable);
        var expectedAvailableSeats = tables.Where(t => t.IsAvailable).Sum(t => t.Seats);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(stats.AvailableTables, Is.EqualTo(expectedAvailableTables.ToString()),
                "Available tables statistic should match actual available tables count");
            Assert.That(stats.AvailableSeats, Is.EqualTo(expectedAvailableSeats.ToString()),
                "Available seats statistic should match sum of seats from available tables");
        });
    }

    [Test]
    public async Task Dashboard_PendingReservationShouldHaveApproveButton()
    {
        // Arrange
        var credentials = TestDataFactory.GetValidUserCredentials();
        await _loginPage.GotoAsync();
        await _loginPage.LoginAsync(credentials.Email, credentials.Password);
        await _dashboardPage.WaitForLoadAsync();
        await _dashboardPage.PendingReservations.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Act
        var reservations = await _dashboardPage.PendingReservations.GetAllReservationsAsync();

        // Assert
        Assert.That(reservations.Count, Is.GreaterThan(0), "Should have at least one reservation");
        
        var pendingReservation = reservations.FirstOrDefault(r => r.Status == "Pending");
        Assert.That(pendingReservation, Is.Not.Null, "Should have a pending reservation");
        Assert.That(pendingReservation!.CanApprove, Is.True,
            "Pending reservation should have an Approve button");
    }
}