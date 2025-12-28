using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class DeleteEmployeeUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToEmployeesAsync();
        await _editPage.Employees.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    private async Task<string> CreateTestEmployeeAsync()
    {
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var email = $"delete.test.{uniqueId}@example.com";
        
        var employeeData = new AddEmployeeFormData
        {
            FirstName = "Delete",
            LastName = "Test",
            Email = email,
            Role = "Employee"
        };

        await _editPage.Employees.OpenAddEmployeeModalAsync();
        await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
        await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
        await _editPage.Employees.AddEmployeeModal.SaveAndWaitForSuccessAsync();
        await WaitForBlazorAsync();
        await _editPage.Employees.CloseSuccessModalAsync();
        await WaitForBlazorAsync();

        return email;
    }

    [Test]
    public async Task DeleteEmployee_ConfirmDeletion_RemovesFromList()
    {
        // Arrange
        var employeeEmail = await CreateTestEmployeeAsync();
        var initialCount = await _editPage.Employees.GetEmployeeCountAsync();
        
        Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(employeeEmail), Is.True,
            "Employee should exist before deletion");

        // Act
        await _editPage.Employees.DeleteEmployeeAsync(employeeEmail);
        await _editPage.Employees.ConfirmDeleteAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Employees.GetEmployeeCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount - 1),
                "Employee count should decrease by 1");
            Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(employeeEmail), Is.False,
                "Deleted employee should not be visible");
        });
    }

    [Test]
    public async Task DeleteEmployee_CancelDeletion_EmployeeRemains()
    {
        // Arrange
        var employeeEmail = await CreateTestEmployeeAsync();
        var initialCount = await _editPage.Employees.GetEmployeeCountAsync();

        // Act
        await _editPage.Employees.DeleteEmployeeAsync(employeeEmail);
        await _editPage.Employees.CancelDeleteAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Employees.GetEmployeeCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount),
                "Employee count should remain the same");
            Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(employeeEmail), Is.True,
                "Employee should still be visible after cancel");
        });
    }

    [Test]
    public async Task DeleteEmployee_PersistsAfterRefresh()
    {
        // Arrange
        var employeeEmail = await CreateTestEmployeeAsync();

        // Act
        await _editPage.Employees.DeleteEmployeeAsync(employeeEmail);
        await _editPage.Employees.ConfirmDeleteAsync();
        await WaitForBlazorAsync();

        // Refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToEmployeesAsync();
        await _editPage.Employees.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(employeeEmail), Is.False,
            "Deleted employee should not reappear after refresh");
    }
    
}