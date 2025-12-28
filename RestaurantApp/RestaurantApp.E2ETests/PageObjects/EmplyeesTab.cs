using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class EmployeesTab
{
    private readonly IPage _page;
    
    public AddEmployeeModal AddEmployeeModal { get; }
    public InviteEmployeeModal InviteEmployeeModal { get; }
    public ManagePermissionsModal PermissionsModal { get; }

    public EmployeesTab(IPage page)
    {
        _page = page;
        AddEmployeeModal = new AddEmployeeModal(page);
        InviteEmployeeModal = new InviteEmployeeModal(page);
        PermissionsModal = new ManagePermissionsModal(page);
    }

    // Locators
    private ILocator AddEmployeeButton => _page.Locator("button:has-text('Add new employee')");
    private ILocator InviteEmployeeButton => _page.Locator("button:has-text('Invite employees')");
    private ILocator EmployeeCards => _page.Locator(".card.reservation-card");
    private ILocator LoadingSpinner => _page.Locator(".spinner-border:has-text('Loading employees')");
    private ILocator ErrorAlert => _page.Locator(".alert-danger");
    private ILocator NoEmployeesAlert => _page.Locator(".alert:has-text(\"don't have any employees\")");
    private ILocator DeleteConfirmModal => _page.Locator(".modal:has-text('Delete Employee')");
    private ILocator SuccessModal => _page.Locator(".modal:has-text('Employee added successfully')");

    // Actions
    public async Task OpenAddEmployeeModalAsync()
    {
        await AddEmployeeButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show");
    }

    public async Task OpenInviteEmployeeModalAsync()
    {
        await InviteEmployeeButton.ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show");
    }

    public async Task OpenPermissionsForEmployeeAsync(string employeeEmail)
    {
        var card = GetEmployeeCard(employeeEmail);
        await card.Locator("button:has-text('Manage Permissions')").ClickAsync();
        await _page.WaitForSelectorAsync(".modal.show");
    }

    public async Task DeleteEmployeeAsync(string employeeEmail)
    {
        var card = GetEmployeeCard(employeeEmail);
        await card.Locator("button:has(.bi-trash)").ClickAsync();
        await _page.WaitForSelectorAsync(".modal:has-text('Delete Employee')");
    }

    public async Task ConfirmDeleteAsync()
    {
        await DeleteConfirmModal.Locator("button:has-text('Delete')").ClickAsync();
        await _page.WaitForSelectorAsync(".modal", new() { State = WaitForSelectorState.Hidden });
    }

    public async Task CancelDeleteAsync()
    {
        await DeleteConfirmModal.Locator("button:has-text('Cancel')").ClickAsync();
    }

    public async Task CloseSuccessModalAsync()
    {
        await SuccessModal.Locator("button:has-text('OK')").ClickAsync();
    }

    // State checks
    public async Task<int> GetEmployeeCountAsync() 
        => await EmployeeCards.CountAsync();

    public async Task<bool> IsLoadingAsync() 
        => await LoadingSpinner.IsVisibleAsync();

    public async Task<bool> HasErrorAsync() 
        => await ErrorAlert.IsVisibleAsync();

    public async Task<bool> HasNoEmployeesAsync() 
        => await NoEmployeesAlert.IsVisibleAsync();

    public async Task<bool> IsEmployeeVisibleAsync(string email) 
        => await GetEmployeeCard(email).IsVisibleAsync();

    public async Task<List<EmployeeCardData>> GetAllEmployeesAsync()
    {
        var employees = new List<EmployeeCardData>();
        var count = await EmployeeCards.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var card = EmployeeCards.Nth(i);
            employees.Add(new EmployeeCardData
            {
                FullName = await card.Locator(".card-title").InnerTextAsync(),
                Email = await card.Locator("small").First.InnerTextAsync(),
                Role = await GetRoleBadgeTextAsync(card)
            });
        }

        return employees;
    }

    public async Task WaitForLoadAsync()
    {
        await _page.WaitForSelectorAsync(".spinner-border", new() { State = WaitForSelectorState.Hidden });
    }

    // Helpers
    private ILocator GetEmployeeCard(string email) 
        => _page.Locator($".card:has(small:has-text('{email}'))");

    private async Task<string?> GetRoleBadgeTextAsync(ILocator card)
    {
        var badge = card.Locator(".badge");
        return await badge.IsVisibleAsync() ? await badge.InnerTextAsync() : null;
    }
    public async Task<bool> IsSuccessModalVisibleAsync()
        => await SuccessModal.IsVisibleAsync();

    public async Task<string?> GetSuccessModalEmailAsync()
    {
        if (!await SuccessModal.IsVisibleAsync())
            return null;
    
        var emailElement = SuccessModal.Locator("strong");
        if (await emailElement.IsVisibleAsync())
            return await emailElement.InnerTextAsync();
    
        return null;
    }
}

public record EmployeeCardData
{
    public string FullName { get; init; } = "";
    public string Email { get; init; } = "";
    public string? Role { get; init; }
}