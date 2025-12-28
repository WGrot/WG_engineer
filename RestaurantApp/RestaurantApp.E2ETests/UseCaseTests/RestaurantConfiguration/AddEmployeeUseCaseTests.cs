using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

public class AddEmployeeUseCaseTests: PlaywrightTestBase
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

    [Test]
    public async Task AddEmployee_WithAllFields_Success()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var employeeData = new AddEmployeeFormData
        {
            FirstName = "Test",
            LastName = "Employee",
            Email = $"test.employee.{uniqueId}@example.com",
            PhoneNumber = "123456789",
            Role = "Manager"
        };
        
        var initialCount = await _editPage.Employees.GetEmployeeCountAsync();

        // Act
        await _editPage.Employees.OpenAddEmployeeModalAsync();
        await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
        await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
        await _editPage.Employees.AddEmployeeModal.SaveAndWaitForSuccessAsync();
        await WaitForBlazorAsync();

        // Handle success modal
        Assert.That(await _editPage.Employees.IsSuccessModalVisibleAsync(), Is.True,
            "Success modal should be visible");
        await _editPage.Employees.CloseSuccessModalAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Employees.GetEmployeeCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount + 1),
            "Employee count should increase by 1");
    }
    
    [Test]
    public async Task AddEmployee_AppearsInEmployeeList()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var employeeData = new AddEmployeeFormData
        {
            FirstName = "Listed",
            LastName = "Employee",
            Email = $"listed.{uniqueId}@example.com",
            PhoneNumber = "987654321",
            Role = "Waiter"
        };

        // Act
        await _editPage.Employees.OpenAddEmployeeModalAsync();
        await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
        await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
        await _editPage.Employees.AddEmployeeModal.SaveAndWaitForSuccessAsync();
        await WaitForBlazorAsync();
        await _editPage.Employees.CloseSuccessModalAsync();
        await WaitForBlazorAsync();

        // Assert
        var employees = await _editPage.Employees.GetAllEmployeesAsync();
        var addedEmployee = employees.FirstOrDefault(e => e.Email == employeeData.Email);
        
        Assert.That(addedEmployee, Is.Not.Null, "Added employee should be in the list");
        Assert.Multiple(() =>
        {
            Assert.That(addedEmployee!.FullName, Does.Contain(employeeData.FirstName),
                "Employee name should contain first name");
            Assert.That(addedEmployee.FullName, Does.Contain(employeeData.LastName),
                "Employee name should contain last name");
        });
    }

    [Test]
    public async Task AddEmployee_PersistsAfterRefresh()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var email = $"persist.{uniqueId}@example.com";
        var employeeData = new AddEmployeeFormData
        {
            FirstName = "Persist",
            LastName = "Test",
            Email = email,
            Role = "Manager"
        };

        // Act
        await _editPage.Employees.OpenAddEmployeeModalAsync();
        await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
        await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
        await _editPage.Employees.AddEmployeeModal.SaveAndWaitForSuccessAsync();
        await WaitForBlazorAsync();
        await _editPage.Employees.CloseSuccessModalAsync();
        await WaitForBlazorAsync();

        // Refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToEmployeesAsync();
        await _editPage.Employees.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(email), Is.True,
            "Employee should persist after page refresh");
    }

    [Test]
    public async Task AddEmployee_WithDifferentRoles_Success()
    {
        // Test adding employees with different roles
        var roles = new[] { "Manager", "Waiter", "Chef" }; // Adjust based on your actual roles

        foreach (var role in roles)
        {
            var uniqueId = Guid.NewGuid().ToString("N")[..8];
            var employeeData = new AddEmployeeFormData
            {
                FirstName = $"Role{role}",
                LastName = "Test",
                Email = $"role.{role.ToLower()}.{uniqueId}@example.com",
                Role = role
            };

            await _editPage.Employees.OpenAddEmployeeModalAsync();
            await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
            await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
            await _editPage.Employees.AddEmployeeModal.SaveAndWaitForSuccessAsync();
            await WaitForBlazorAsync();
            await _editPage.Employees.CloseSuccessModalAsync();
            await WaitForBlazorAsync();

            Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(employeeData.Email), Is.True,
                $"Employee with role {role} should be added successfully");
        }
    }
    

    [Test]
    public async Task AddEmployeeModal_OpensCorrectly()
    {
        // Act
        await _editPage.Employees.OpenAddEmployeeModalAsync();

        // Assert
        Assert.That(await _editPage.Employees.AddEmployeeModal.IsVisibleAsync(), Is.True,
            "Add employee modal should be visible");
        
        var title = await _editPage.Employees.AddEmployeeModal.GetTitleAsync();
        Assert.That(title, Does.Contain("Add new employee"),
            "Modal title should contain 'Add new employee'");
    }

    [Test]
    public async Task AddEmployeeModal_CloseWithCancelButton()
    {
        // Arrange
        await _editPage.Employees.OpenAddEmployeeModalAsync();
        Assert.That(await _editPage.Employees.AddEmployeeModal.IsVisibleAsync(), Is.True);

        // Act
        await _editPage.Employees.AddEmployeeModal.CloseAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Employees.AddEmployeeModal.IsVisibleAsync(), Is.False,
            "Modal should be closed after clicking Cancel");
    }

    [Test]
    public async Task AddEmployeeModal_CloseWithXButton()
    {
        // Arrange
        await _editPage.Employees.OpenAddEmployeeModalAsync();
        
        // Act - Use close button directly
        var closeButton = Page.Locator(".modal.show .btn-close");
        await closeButton.ClickAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Employees.AddEmployeeModal.IsVisibleAsync(), Is.False,
            "Modal should be closed after clicking X button");
    }

    [Test]
    public async Task AddEmployeeModal_CancelDoesNotSaveData()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N")[..8];
        var email = $"cancel.test.{uniqueId}@example.com";
        var employeeData = new AddEmployeeFormData
        {
            FirstName = "Cancel",
            LastName = "Test",
            Email = email,
            Role = "Manager"
        };
        
        var initialCount = await _editPage.Employees.GetEmployeeCountAsync();

        // Act
        await _editPage.Employees.OpenAddEmployeeModalAsync();
        await _editPage.Employees.AddEmployeeModal.WaitForVisibleAsync();
        await _editPage.Employees.AddEmployeeModal.FillFormAsync(employeeData);
        await _editPage.Employees.AddEmployeeModal.CloseAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Employees.GetEmployeeCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount),
            "Employee count should not change after cancel");
        Assert.That(await _editPage.Employees.IsEmployeeVisibleAsync(email), Is.False,
            "Cancelled employee should not appear in list");
    }
    
}