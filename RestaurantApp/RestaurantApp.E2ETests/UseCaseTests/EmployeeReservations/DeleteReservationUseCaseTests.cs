using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;

[TestFixture]
public class DeleteReservationUseCaseTests : PlaywrightTestBase
{
    private ManageReservationsPage _manageReservationsPage = null!;
    private ReservationDetailsModal _reservationDetailsModal = null!;

    [SetUp]
    public async Task Setup()
    {
        _manageReservationsPage = new ManageReservationsPage(Page);
        _reservationDetailsModal = new ReservationDetailsModal(Page);

        var credentials = TestDataFactory.GetTestUserCredentials(6);
        await LoginAsUserAsync(credentials.Email, credentials.Password);
        await _manageReservationsPage.GotoAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();
    }
    
    
    [Test]
    public async Task DeleteReservation_ShouldRemoveReservationFromList()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        var initialCount = await _manageReservationsPage.GetReservationCountAsync();
        var reservationToDelete = await _manageReservationsPage.GetReservationCardDataAsync(0);

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();

        await _reservationDetailsModal.DeleteReservationAsync();
        await WaitForBlazorAsync();

        await _manageReservationsPage.SuccessToast.WaitForAsync(new() { Timeout = 5000 });
    
        Assert.That(await _manageReservationsPage.SuccessToast.IsVisibleAsync(), Is.True,
            "Success toast should be displayed after deleting reservation");

        var newCount = await _manageReservationsPage.GetReservationCountAsync();
    
        Assert.That(newCount, Is.EqualTo(initialCount - 1),
            $"Reservation count should decrease by 1. Initial: {initialCount}, After delete: {newCount}");
    }
    
    
    [Test]
    public async Task DeleteReservation_CancelConfirmation_ShouldNotRemoveReservation()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        var initialCount = await _manageReservationsPage.GetReservationCountAsync();

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();

        await _reservationDetailsModal.ClickDeleteAsync();
        await _reservationDetailsModal.CancelDeleteAsync();
        await WaitForBlazorAsync();

        Assert.That(await _reservationDetailsModal.IsVisibleAsync(), Is.True,
            "Reservation details modal should remain open after cancelling delete");

        await _reservationDetailsModal.CloseAsync();
        await WaitForBlazorAsync();

        var newCount = await _manageReservationsPage.GetReservationCountAsync();

        Assert.That(newCount, Is.EqualTo(initialCount),
            $"Reservation count should remain the same. Initial: {initialCount}, After cancel: {newCount}");
    }
}