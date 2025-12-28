// PageObjects/Modals/ReservationDetailsModal.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.Dashboard;

public class ReservationDetailsModal
{
    private readonly IPage _page;

    public ReservationDetailsModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal.show:has(.modal-title:has-text('Manage Reservation'))");
    private ILocator CloseButton => Modal.Locator("button:has-text('Close')");
    
    // Reservation details
    private ILocator CustomerNameText => Modal.Locator("p:has-text('Customer:')");
    private ILocator CustomerEmailText => Modal.Locator("p:has-text('Email:')");
    private ILocator CustomerPhoneText => Modal.Locator("p:has-text('Phone:')");
    private ILocator DateText => Modal.Locator("p:has-text('Date:')");
    private ILocator TimeText => Modal.Locator("p:has-text('Time:')");
    private ILocator TableNumberText => Modal.Locator("p:has-text('Table number:')");
    private ILocator GuestsText => Modal.Locator("p:has-text('Guests:')");
    private ILocator CurrentStatusBadge => Modal.Locator("p:has-text('Current Status:') .badge");
    
    // Actions
    private ILocator StatusSelect => Modal.Locator("#statusSelect");
    private ILocator UpdateStatusButton => Modal.Locator("button:has-text('Update Status')");
    private ILocator DeleteButton => Modal.Locator("button:has-text('Delete')");
    private ILocator Spinner => Modal.Locator(".spinner-border");
    
    // Alerts
    private ILocator ErrorAlert => Modal.Locator(".alert-danger");
    private ILocator SuccessAlert => Modal.Locator(".alert-success");
    
    // Confirmation modal
    private ILocator ConfirmDeleteModal => _page.Locator(".modal.show:has-text('Confirm Deletion')");
    private ILocator ConfirmDeleteButton => ConfirmDeleteModal.Locator("button:has-text('Confirm'), button:has-text('Delete')");
    private ILocator CancelDeleteButton => ConfirmDeleteModal.Locator("button:has-text('Cancel')");

    public async Task WaitForVisibleAsync()
    {
        await Modal.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task<bool> IsVisibleAsync()
    {
        return await Modal.IsVisibleAsync();
    }

    public async Task CloseAsync()
    {
        await CloseButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    // Get reservation details
    public async Task<ReservationDetailsData> GetReservationDetailsAsync()
    {
        var data = new ReservationDetailsData();

        data.CustomerName = await GetTextValueAsync(CustomerNameText, "Customer:");
        data.CustomerEmail = await GetTextValueAsync(CustomerEmailText, "Email:");
        data.CustomerPhone = await GetTextValueAsync(CustomerPhoneText, "Phone:");
        data.Date = await GetTextValueAsync(DateText, "Date:");
        data.Time = await GetTextValueAsync(TimeText, "Time:");
        data.TableNumber = await GetTextValueAsync(TableNumberText, "Table number:");
        data.Guests = await GetTextValueAsync(GuestsText, "Guests:");
        data.Status = (await CurrentStatusBadge.InnerTextAsync()).Trim();

        return data;
    }

    private async Task<string> GetTextValueAsync(ILocator locator, string prefix)
    {
        if (!await locator.IsVisibleAsync())
            return "";
        
        var text = await locator.InnerTextAsync();
        return text.Replace(prefix, "").Trim();
    }

    public async Task<string> GetCurrentStatusAsync()
    {
        return (await CurrentStatusBadge.InnerTextAsync()).Trim();
    }

    // Status change
    public async Task SelectStatusAsync(string status)
    {
        await StatusSelect.SelectOptionAsync(status);
    }

    public async Task UpdateStatusAsync(string newStatus)
    {
        await SelectStatusAsync(newStatus);
        await UpdateStatusButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task UpdateStatusAndWaitForResponseAsync(string newStatus)
    {
        await SelectStatusAsync(newStatus);
        await _page.RunAndWaitForResponseAsync(
            async () => await UpdateStatusButton.ClickAsync(),
            r => r.Url.Contains("Reservation") && (r.Request.Method == "PUT" || r.Request.Method == "PATCH"),
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(500);
    }

    // Delete reservation
    public async Task ClickDeleteAsync()
    {
        await DeleteButton.ClickAsync();
        await ConfirmDeleteModal.WaitForAsync(new() { Timeout = 3000 });
    }

    public async Task ConfirmDeleteAsync()
    {
        await ConfirmDeleteButton.ClickAsync();
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task CancelDeleteAsync()
    {
        await CancelDeleteButton.ClickAsync();
        await ConfirmDeleteModal.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 3000 });
    }

    public async Task DeleteReservationAsync()
    {
        await ClickDeleteAsync();
        await ConfirmDeleteAsync();
    }

    // State checks
    public async Task<bool> IsProcessingAsync()
    {
        return await Spinner.IsVisibleAsync();
    }

    public async Task<bool> HasErrorAsync()
    {
        return await ErrorAlert.IsVisibleAsync();
    }

    public async Task<bool> HasSuccessAsync()
    {
        return await SuccessAlert.IsVisibleAsync();
    }

    public async Task<string?> GetErrorMessageAsync()
    {
        if (!await ErrorAlert.IsVisibleAsync())
            return null;
        return (await ErrorAlert.InnerTextAsync()).Trim();
    }

    public async Task<string?> GetSuccessMessageAsync()
    {
        if (!await SuccessAlert.IsVisibleAsync())
            return null;
        return (await SuccessAlert.InnerTextAsync()).Trim();
    }
}

public record ReservationDetailsData
{
    public string CustomerName { get; set; } = "";
    public string CustomerEmail { get; set; } = "";
    public string CustomerPhone { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public string TableNumber { get; set; } = "";
    public string Guests { get; set; } = "";
    public string Status { get; set; } = "";
}