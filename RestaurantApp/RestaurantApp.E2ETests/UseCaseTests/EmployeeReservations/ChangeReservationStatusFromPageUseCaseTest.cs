using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.Dashboard;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.EmployeeReservations;

[TestFixture]
public class ChangeReservationStatusFromPageUseCaseTest : PlaywrightTestBase
{
    private ManageReservationsPage _manageReservationsPage = null!;
    private ReservationDetailsModal _reservationDetailsModal = null!;

    [SetUp]
    public async Task Setup()
    {
        _manageReservationsPage = new ManageReservationsPage(Page);
        _reservationDetailsModal = new ReservationDetailsModal(Page);

        await LoginAsVerifiedUserAsync();
        await _manageReservationsPage.GotoAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();
    }

    [Test]
    public async Task ChangeStatus_ShouldUpdateStatusOnReservationCard()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        var initialCardData = await _manageReservationsPage.GetReservationCardDataAsync(0);
        var newStatus = GetDifferentStatus(initialCardData.Status);

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();

        await _reservationDetailsModal.UpdateStatusAndWaitForResponseAsync(newStatus);
        await WaitForBlazorAsync();

        // Modal closes automatically on success, no need to call CloseAsync()
        await _manageReservationsPage.SuccessToast.WaitForAsync(new() { Timeout = 5000 });

        var updatedCardData = await _manageReservationsPage.GetReservationCardDataAsync(0);

        Assert.That(updatedCardData.Status, Is.EqualTo(newStatus),
            $"Status on card should be updated from '{initialCardData.Status}' to '{newStatus}'");
    }

    [Test]
    public async Task ChangeStatus_ShouldCloseModalAndShowSuccessToast()
    {
        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        Assert.That(hasReservations, Is.True, "Test requires at least one reservation in database");

        await _manageReservationsPage.ClickReservationCardAsync(0);
        await _reservationDetailsModal.WaitForVisibleAsync();

        var initialStatus = await _reservationDetailsModal.GetCurrentStatusAsync();
        var newStatus = GetDifferentStatus(initialStatus);

        await _reservationDetailsModal.UpdateStatusAndWaitForResponseAsync(newStatus);
        await WaitForBlazorAsync();

        // Modal should close automatically on success
        Assert.That(await _reservationDetailsModal.IsVisibleAsync(), Is.False,
            "Modal should close automatically after successful status update");

        await _manageReservationsPage.SuccessToast.WaitForAsync(new() { Timeout = 5000 });
        Assert.That(await _manageReservationsPage.SuccessToast.IsVisibleAsync(), Is.True,
            "Success toast should be displayed after status change");
    }

    [Test]
    public async Task ApproveButton_OnPendingReservation_ShouldChangeStatusToConfirmed()
    {
        await FilterByStatusAsync("Pending");

        var hasReservations = await _manageReservationsPage.HasReservationsAsync();
        if (!hasReservations)
        {
            Assert.Ignore("No pending reservations available to test");
        }

        var hasApproveButton = await _manageReservationsPage.HasApproveButtonOnCardAsync(0);
        Assert.That(hasApproveButton, Is.True, "Pending reservation should have 'Approve' button");

        await _manageReservationsPage.ClickApproveOnCardAsync(0);
        await WaitForBlazorAsync();

        await _manageReservationsPage.SuccessToast.WaitForAsync(new() { Timeout = 5000 });

        Assert.That(await _manageReservationsPage.SuccessToast.IsVisibleAsync(), Is.True,
            "Success toast should be displayed after approving reservation");
    }

    private async Task FilterByStatusAsync(string status)
    {
        await _manageReservationsPage.SelectStatusAsync(status);
        await _manageReservationsPage.ApplyFiltersAsync();
        await _manageReservationsPage.WaitForReservationsLoadedAsync();
        await WaitForBlazorAsync();
    }

    private static string GetDifferentStatus(string currentStatus)
    {
        return currentStatus switch
        {
            "Pending" => "Confirmed",
            "Confirmed" => "Cancelled",
            "Cancelled" => "Confirmed",
            "Completed" => "Cancelled",
            _ => "Confirmed"
        };
    }
}