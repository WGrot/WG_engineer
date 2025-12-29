using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class ManagePermissionsUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;
    private string _testEmployeeEmail = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToEmployeesAsync();
        await _editPage.Employees.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Create a test employee for permission tests
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        _testEmployeeEmail = $"perm.test.{uniqueId}@example.com";
        
        await CreateTestEmployeeAsync(_testEmployeeEmail);
    }

    [TearDown]
    public async Task Cleanup()
    {
        try
        {
            // Delete test employee if exists
            await _editPage.NavigateAsync(1);
            await _editPage.SwitchToEmployeesAsync();
            await _editPage.Employees.WaitForLoadAsync();
            await WaitForBlazorAsync();

            if (await _editPage.Employees.IsEmployeeVisibleAsync(_testEmployeeEmail))
            {
                await _editPage.Employees.DeleteEmployeeAsync(_testEmployeeEmail);
                await _editPage.Employees.ConfirmDeleteAsync();
                await WaitForBlazorAsync();
            }
        }
        catch { /* Ignore cleanup errors */ }
    }

    private async Task CreateTestEmployeeAsync(string email)
    {
        var employeeData = new AddEmployeeFormData
        {
            FirstName = "Permission",
            LastName = "Test",
            Email = email,
            Role = "Employee"  // Start with basic role
        };

        await _editPage.Employees.OpenAddEmployeeModalAsync();
        await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
        await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
        await _editPage.Employees.AddEmployeeModal.SaveAndWaitForSuccessAsync();
        await WaitForBlazorAsync();
        await _editPage.Employees.CloseSuccessModalAsync();
        await WaitForBlazorAsync();
    }

    #region Open Modal Tests

    [Test]
    public async Task OpenPermissionsModal_DisplaysCorrectEmployeeInfo()
    {
        // Act
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Assert
        Assert.That(await _editPage.Employees.PermissionsModal.IsVisibleAsync(), Is.True,
            "Permissions modal should be visible");

        var employeeName = await _editPage.Employees.PermissionsModal.GetEmployeeNameFromTitleAsync();
        Assert.That(employeeName, Does.Contain("Permission"),
            "Modal title should contain employee name");

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }

    [Test]
    public async Task OpenPermissionsModal_ShowsCurrentRole()
    {
        // Act
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Assert
        var currentRole = await _editPage.Employees.PermissionsModal.GetCurrentRoleAsync();
        Assert.That(currentRole, Is.Not.Empty,
            "Current role should be displayed");

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }

    #endregion

    #region Enable/Disable Permission Tests

    [Test]
    public async Task EnablePermission_ManageReservations_Success()
    {
        // Arrange
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Act
        await _editPage.Employees.PermissionsModal.EnablePermissionAsync(PermissionType.ManageReservations);
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Assert - Reopen modal to verify
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        Assert.That(
            await _editPage.Employees.PermissionsModal.IsPermissionEnabledAsync(PermissionType.ManageReservations),
            Is.True,
            "ManageReservations permission should be enabled");

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }

    [Test]
    public async Task DisablePermission_Success()
    {
        // Arrange - First enable a permission
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();
        await _editPage.Employees.PermissionsModal.EnablePermissionAsync(PermissionType.ManageTables);
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Act - Now disable it
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();
        await _editPage.Employees.PermissionsModal.DisablePermissionAsync(PermissionType.ManageTables);
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Assert - Reopen to verify
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        Assert.That(
            await _editPage.Employees.PermissionsModal.IsPermissionEnabledAsync(PermissionType.ManageTables),
            Is.False,
            "ManageTables permission should be disabled");

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }

    [Test]
    public async Task EnableMultiplePermissions_Success()
    {
        // Arrange
        var permissionsToEnable = new[]
        {
            PermissionType.ManageReservations,
            PermissionType.ManageTables,
            PermissionType.ManageMenu
        };

        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Act
        foreach (var permission in permissionsToEnable)
        {
            await _editPage.Employees.PermissionsModal.EnablePermissionAsync(permission);
        }
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Assert - Reopen to verify
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        var enabledPermissions = await _editPage.Employees.PermissionsModal.GetEnabledPermissionsAsync();
        
        Assert.Multiple(() =>
        {
            foreach (var permission in permissionsToEnable)
            {
                Assert.That(enabledPermissions, Does.Contain(permission),
                    $"Permission {permission} should be enabled");
            }
        });

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }
    
    #endregion

    #region Change Role Tests

    [Test]
    public async Task ChangeRole_FromEmployeeToManager_Success()
    {
        // Arrange
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Act
        await _editPage.Employees.PermissionsModal.SelectRoleAsync("Manager");
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Assert - Reopen to verify
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        var currentRole = await _editPage.Employees.PermissionsModal.GetCurrentRoleAsync();
        Assert.That(currentRole, Is.EqualTo("Manager"),
            "Role should be changed to Manager");

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }

    [Test]
    public async Task ChangeRole_PersistsAfterRefresh()
    {
        // Arrange
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Act
        await _editPage.Employees.PermissionsModal.SelectRoleAsync("Manager");
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToEmployeesAsync();
        await _editPage.Employees.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var employees = await _editPage.Employees.GetAllEmployeesAsync();
        var employee = employees.FirstOrDefault(e => e.Email == _testEmployeeEmail);
        
        Assert.That(employee, Is.Not.Null, "Employee should exist");
        Assert.That(employee!.Role, Is.EqualTo("Manager"),
            "Role should persist after refresh");
    }

    #endregion

    #region Account Active Tests

    [Test]
    public async Task DeactivateAccount_Success()
    {
        // Arrange
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Verify account is initially active
        Assert.That(await _editPage.Employees.PermissionsModal.IsAccountActiveAsync(), Is.True,
            "Account should be initially active");

        // Act
        await _editPage.Employees.PermissionsModal.SetAccountActiveAsync(false);
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Assert - Reopen to verify
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        Assert.That(await _editPage.Employees.PermissionsModal.IsAccountActiveAsync(), Is.False,
            "Account should be deactivated");

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }
    #endregion

    #region Cancel Without Save Tests
    

    #endregion

    #region Combined Changes Tests
    

    #endregion

    #region Permissions Persist After Refresh Tests

    [Test]
    public async Task PermissionChanges_PersistAfterPageRefresh()
    {
        // Arrange
        var permissionsToSet = new[]
        {
            PermissionType.ManageReservations,
            PermissionType.ManageMenu,
            PermissionType.ManageTables
        };

        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        // Act
        await _editPage.Employees.PermissionsModal.SetPermissionsAsync(permissionsToSet);
        await _editPage.Employees.PermissionsModal.SaveAndCloseAsync();
        await WaitForBlazorAsync();

        // Refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToEmployeesAsync();
        await _editPage.Employees.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        await _editPage.Employees.OpenPermissionsForEmployeeAsync(_testEmployeeEmail);
        await _editPage.Employees.PermissionsModal.WaitForVisibleAsync();
        await _editPage.Employees.PermissionsModal.WaitForLoadAsync();

        var enabledPermissions = await _editPage.Employees.PermissionsModal.GetEnabledPermissionsAsync();

        Assert.Multiple(() =>
        {
            foreach (var permission in permissionsToSet)
            {
                Assert.That(enabledPermissions, Does.Contain(permission),
                    $"Permission {permission} should persist after refresh");
            }
        });

        // Cleanup
        await _editPage.Employees.PermissionsModal.CloseAsync();
    }

    #endregion
}