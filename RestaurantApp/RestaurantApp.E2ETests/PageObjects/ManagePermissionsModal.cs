using Microsoft.Playwright;

namespace RestaurantApp.E2ETests.PageObjects;

public class ManagePermissionsModal
{
    private readonly IPage _page;

    public ManagePermissionsModal(IPage page) => _page = page;

    private ILocator Modal => _page.Locator(".modal:has-text('Manage permissions')");
    private ILocator RoleSelect => Modal.Locator("select");
    private ILocator SaveButton => Modal.Locator("button:has-text('Save changes')");
    private ILocator CancelButton => Modal.Locator("button:has-text('Cancel')");
    private ILocator CloseButton => Modal.Locator(".btn-close");
    private ILocator Spinner => Modal.Locator(".spinner-border");
    private ILocator LoadingSpinner => Modal.Locator(".spinner-border:has-text('Loading permissions')");
    private ILocator IsActiveCheckbox => Modal.Locator("#isActive");

    // Employee info card
    private ILocator EmployeeInfoCard => Modal.Locator(".card").First;
    private ILocator CurrentRoleBadge => Modal.Locator(".badge.bg-primary");

    // Permission checkboxes
    private ILocator GetPermissionCheckbox(PermissionType permission) 
        => Modal.Locator($"#permission-{permission}");

    private ILocator AllPermissionCheckboxes 
        => Modal.Locator(".form-check-input[id^='permission-']");

    // Actions
    public async Task SelectRoleAsync(RestaurantRole role)
    {
        await RoleSelect.SelectOptionAsync(role.ToString());
    }

    public async Task SetPermissionAsync(PermissionType permission, bool enabled)
    {
        var checkbox = GetPermissionCheckbox(permission);
        var isChecked = await checkbox.IsCheckedAsync();

        if (isChecked != enabled)
            await checkbox.ClickAsync();
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
        // Wait for both API calls (permissions + role/details)
        await _page.WaitForResponseAsync(r => r.Url.Contains("Permissions"));
    }

    public async Task SaveAndCloseAsync()
    {
        await SaveAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public async Task CloseAsync()
    {
        await CancelButton.ClickAsync();
        await Modal.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
    }

    public async Task WaitForLoadAsync()
    {
        await LoadingSpinner.WaitForAsync(new() { State = WaitForSelectorState.Hidden });
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
        => await CurrentRoleBadge.InnerTextAsync();

    public async Task<string> GetSelectedRoleAsync() 
        => await RoleSelect.InputValueAsync();

    public async Task<EmployeeInfoData> GetEmployeeInfoAsync()
    {
        var cardText = await EmployeeInfoCard.InnerTextAsync();
        // Parse from card - simplified
        return new EmployeeInfoData
        {
            FullName = await Modal.Locator(".modal-title").InnerTextAsync(),
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
}

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