using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class ManagePermissionsModal
{
    private readonly IPage _page;

    public ManagePermissionsModal(IPage page) => _page = page;

    // Fixed: Use .modal.show to target visible modal
    private ILocator Modal => _page.Locator(".modal.show:has-text('Manage permissions')");
    private ILocator RoleSelect => Modal.Locator("select");
    private ILocator SaveButton => Modal.Locator("button:has-text('Save changes')");
    private ILocator CancelButton => Modal.Locator("button:has-text('Cancel')");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    private ILocator Spinner => Modal.Locator(".spinner-border");
    private ILocator LoadingSpinner => Modal.Locator(".spinner-border");
    private ILocator IsActiveCheckbox => Modal.Locator("#isActive");

    // Employee info card
    private ILocator EmployeeInfoCard => Modal.Locator(".card").First;
    private ILocator CurrentRoleBadge => Modal.Locator(".badge.bg-primary");

    // Permission checkboxes - match the actual IDs from Blazor component
    private ILocator GetPermissionCheckbox(PermissionType permission) 
        => Modal.Locator($"#permission-{permission}");

    private ILocator AllPermissionCheckboxes 
        => Modal.Locator(".form-check-input[id^='permission-']");

    // Actions
    public async Task WaitForVisibleAsync()
    {
        await Modal.WaitForAsync(new() { Timeout = 5000 });
    }

    public async Task WaitForLoadAsync()
    {
        // Wait for loading spinner to appear and then disappear
        try
        {
            await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 1000 });
        }
        catch (TimeoutException) { }
        
        await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 10000 });
        await _page.WaitForTimeoutAsync(300);
    }

    public async Task SelectRoleAsync(string role)
    {
        await RoleSelect.SelectOptionAsync(role);
    }

    public async Task SetPermissionAsync(PermissionType permission, bool enabled)
    {
        var checkbox = GetPermissionCheckbox(permission);
        await checkbox.WaitForAsync(new() { Timeout = 5000 });
        var isChecked = await checkbox.IsCheckedAsync();

        if (isChecked != enabled)
            await checkbox.ClickAsync();
    }

    public async Task EnablePermissionAsync(PermissionType permission)
    {
        await SetPermissionAsync(permission, true);
    }

    public async Task DisablePermissionAsync(PermissionType permission)
    {
        await SetPermissionAsync(permission, false);
    }

    public async Task SetPermissionsAsync(params PermissionType[] permissionsToEnable)
    {
        // First uncheck all
        var allCheckboxes = AllPermissionCheckboxes;
        var count = await allCheckboxes.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var checkbox = allCheckboxes.Nth(i);
            if (await checkbox.IsCheckedAsync())
                await checkbox.ClickAsync();
        }

        // Then enable selected
        foreach (var permission in permissionsToEnable)
        {
            await SetPermissionAsync(permission, true);
        }
    }

    public async Task SetAccountActiveAsync(bool active)
    {
        var isChecked = await IsActiveCheckbox.IsCheckedAsync();
        if (isChecked != active)
            await IsActiveCheckbox.ClickAsync();
    }

    public async Task SaveAsync()
    {
        await SaveButton.ClickAsync();
    }

    public async Task SaveAndWaitForResponseAsync()
    {
        await _page.RunAndWaitForResponseAsync(
            async () => await SaveButton.ClickAsync(),
            r => r.Url.Contains("Permissions") && r.Status == 200,
            new() { Timeout = 10000 });
        await _page.WaitForTimeoutAsync(500);
    }

    public async Task SaveAndCloseAsync()
    {
        await SaveAndWaitForResponseAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden, Timeout = 5000 });
    }

    public async Task CloseAsync()
    {
        await CancelButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    // State checks
    public async Task<bool> IsVisibleAsync() 
        => await Modal.IsVisibleAsync();

    public async Task<bool> IsLoadingAsync() 
        => await LoadingSpinner.IsVisibleAsync();

    public async Task<bool> IsSavingAsync() 
        => await Spinner.IsVisibleAsync();

    public async Task<bool> IsAccountActiveAsync() 
        => await IsActiveCheckbox.IsCheckedAsync();

    public async Task<string> GetCurrentRoleAsync() 
        => (await CurrentRoleBadge.InnerTextAsync()).Trim();

    public async Task<string> GetSelectedRoleAsync() 
        => await RoleSelect.InputValueAsync();

    public async Task<string> GetEmployeeNameFromTitleAsync()
    {
        var title = await Modal.Locator(".modal-title").InnerTextAsync();
        // Title format: "Manage permissions - FirstName LastName"
        return title.Replace("Manage permissions -", "").Trim();
    }

    public async Task<EmployeeInfoData> GetEmployeeInfoAsync()
    {
        var card = EmployeeInfoCard;
        var fullNameElement = card.Locator("p:has-text('Full name:')");
        var emailElement = card.Locator("p:has-text('Email:')");
        var phoneElement = card.Locator("p:has-text('Phone:')");
        
        return new EmployeeInfoData
        {
            FullName = await GetEmployeeNameFromTitleAsync(),
            Email = await emailElement.IsVisibleAsync() 
                ? (await emailElement.InnerTextAsync()).Replace("Email:", "").Trim() 
                : null,
            Phone = await phoneElement.IsVisibleAsync() 
                ? (await phoneElement.InnerTextAsync()).Replace("Phone:", "").Trim() 
                : null,
            CurrentRole = await GetCurrentRoleAsync()
        };
    }

    public async Task<List<PermissionType>> GetEnabledPermissionsAsync()
    {
        var enabled = new List<PermissionType>();

        foreach (PermissionType permission in Enum.GetValues<PermissionType>())
        {
            var checkbox = GetPermissionCheckbox(permission);
            if (await checkbox.IsVisibleAsync() && await checkbox.IsCheckedAsync())
            {
                enabled.Add(permission);
            }
        }

        return enabled;
    }

    public async Task<bool> IsPermissionEnabledAsync(PermissionType permission)
    {
        var checkbox = GetPermissionCheckbox(permission);
        return await checkbox.IsCheckedAsync();
    }

    public async Task<bool> IsSaveButtonEnabledAsync()
    {
        return await SaveButton.IsEnabledAsync();
    }
}

// Must match PermissionTypeEnumDto from backend
public enum PermissionType
{
    ManageRestaurant,
    ManageReservations,
    ManageTables,
    ManageMenu,
    ManageEmployees,
    ManagePermissions,
    ManageRestaurantSettings
}

public record EmployeeInfoData
{
    public string FullName { get; init; } = "";
    public string? Email { get; init; }
    public string? Phone { get; init; }
    public string CurrentRole { get; init; } = "";
}