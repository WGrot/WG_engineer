// PageObjects/Modals/InviteEmployeeModal.cs

using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;

public class InviteEmployeeModal
{
    private readonly IPage _page;

    public InviteEmployeeModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal:has-text('Invite Employee')");
    private ILocator EmailInput => Modal.Locator("input[type='email']");
    private ILocator RoleSelect => Modal.Locator("select");
    private ILocator SendButton => Modal.Locator("button:has-text('Send Invitation')");
    private ILocator CancelButton => Modal.Locator("button:has-text('Cancel')");
    private ILocator DoneButton => Modal.Locator("button:has-text('Done')");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    private ILocator SuccessIcon => Modal.Locator(".bi-check-circle-fill.text-success");
    private ILocator SuccessMessage => Modal.Locator("h5.text-success:has-text('Invitation Sent')");
    private ILocator Spinner => Modal.Locator(".spinner-border");
    private ILocator InfoAlert => Modal.Locator(".alert-info");

    // Actions
    public async Task FillEmailAsync(string email)
    {
        await EmailInput.FillAsync(email);
    }

    public async Task SelectRoleAsync(RestaurantRole role)
    {
        await RoleSelect.SelectOptionAsync(role.ToString());
    }

    public async Task SendInvitationAsync(string email, RestaurantRole role = RestaurantRole.Employee)
    {
        await FillEmailAsync(email);
        await SelectRoleAsync(role);
        await SendButton.ClickAsync();
        await WaitForResponseAsync();
    }

    public async Task CloseAsync()
    {
        if (await DoneButton.IsVisibleAsync())
            await DoneButton.ClickAsync();
        else if (await CancelButton.IsVisibleAsync())
            await CancelButton.ClickAsync();
        else
            await CloseButton.ClickAsync();

        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    // State checks
    public async Task<bool> IsVisibleAsync() 
        => await Modal.IsVisibleAsync();

    public async Task<bool> IsSendingAsync() 
        => await Spinner.IsVisibleAsync();

    public async Task<bool> IsSuccessAsync() 
        => await SuccessIcon.IsVisibleAsync() && await SuccessMessage.IsVisibleAsync();

    public async Task<bool> IsSendButtonEnabledAsync() 
        => await SendButton.IsEnabledAsync();

    public async Task<bool> IsFormDisabledAsync()
        => !await EmailInput.IsEnabledAsync();

    public async Task<string> GetSuccessEmailAsync()
    {
        var text = await Modal.Locator("p:has-text('sent to') strong").InnerTextAsync();
        return text;
    }

    private async Task WaitForResponseAsync()
    {
        await _page.WaitForResponseAsync(r => r.Url.Contains("employeeinvitations"));
    }
}

public enum RestaurantRole
{
    Employee,
    Manager,
    Owner
}