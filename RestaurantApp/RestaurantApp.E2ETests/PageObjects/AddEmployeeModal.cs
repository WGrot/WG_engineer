// PageObjects/Modals/AddEmployeeModal.cs

using Microsoft.Playwright;

public class AddEmployeeModal
{
    private readonly IPage _page;

    public AddEmployeeModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal:has-text('Add new employee')");
    private ILocator FirstNameInput => Modal.Locator("input").First;
    private ILocator LastNameInput => Modal.Locator("input").Nth(1);
    private ILocator EmailInput => Modal.Locator("input[type='email']");
    private ILocator PhoneInput => Modal.Locator("input[type='tel']");
    private ILocator RoleSelect => Modal.Locator("select");
    private ILocator SaveButton => Modal.Locator("button:has-text('Save')");
    private ILocator CancelButton => Modal.Locator("button:has-text('Cancel')");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    private ILocator Spinner => Modal.Locator(".spinner-border");

    // Actions
    public async Task FillFormAsync(AddEmployeeFormData data)
    {
        await FirstNameInput.FillAsync(data.FirstName);
        await LastNameInput.FillAsync(data.LastName);
        await EmailInput.FillAsync(data.Email);
        
        if (!string.IsNullOrEmpty(data.PhoneNumber))
            await PhoneInput.FillAsync(data.PhoneNumber);
        
        if (data.Role.HasValue)
            await RoleSelect.SelectOptionAsync(data.Role.Value.ToString());
    }

    public async Task SaveAsync()
    {
        await SaveButton.ClickAsync();
        await _page.WaitForResponseAsync(r => 
            r.Url.Contains("api/User") && r.Request.Method == "POST");
    }

    public async Task SaveAndWaitForSuccessAsync()
    {
        await SaveAsync();
        // Modal closes after 2 second delay on success
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    public async Task CloseAsync()
    {
        if (await CancelButton.IsVisibleAsync())
            await CancelButton.ClickAsync();
        else
            await CloseButton.ClickAsync();

        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    // State checks
    public async Task<bool> IsVisibleAsync() 
        => await Modal.IsVisibleAsync();

    public async Task<bool> IsSavingAsync() 
        => await Spinner.IsVisibleAsync();

    public async Task<bool> IsSaveButtonDisabledAsync() 
        => !await SaveButton.IsEnabledAsync();

    public async Task<string> GetTitleAsync()
        => await Modal.Locator(".modal-title").InnerTextAsync();
}

public record AddEmployeeFormData
{
    public string FirstName { get; init; } = "";
    public string LastName { get; init; } = "";
    public string Email { get; init; } = "";
    public string? PhoneNumber { get; init; }
    public RestaurantRole? Role { get; init; }
}