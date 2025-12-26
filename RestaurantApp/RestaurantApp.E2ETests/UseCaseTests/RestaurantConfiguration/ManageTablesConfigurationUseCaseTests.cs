using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class ManageTablesConfigurationUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToTablesAsync();
        await _editPage.Tables.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }
    

    #region Add Table Tests

    [Test]
    public async Task AddTable_WithValidData_CreatesNewTable()
    {
        // Arrange
        var initialCount = await _editPage.Tables.GetTableCountAsync();
        var newTable = new TableFormData
        {
            TableNumber = "T-99",
            Capacity = 6,
            Location = "Terrace"
        };

        // Act
        await _editPage.Tables.ClickAddTableAsync();
        await _editPage.Tables.EditTableModal.FillFormAsync(newTable);
        await _editPage.Tables.EditTableModal.SaveAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Tables.GetTableCountAsync();
        Assert.Multiple(() =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount + 1));
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-99").Result, Is.True);
        });
    }

    [Test]
    public async Task AddTable_PersistsAfterReload()
    {
        // Arrange
        var newTable = new TableFormData
        {
            TableNumber = "T-100",
            Capacity = 4,
            Location = "Window"
        };

        // Act
        await _editPage.Tables.ClickAddTableAsync();
        await _editPage.Tables.EditTableModal.FillFormAsync(newTable);
        await _editPage.Tables.EditTableModal.SaveAsync();
        await WaitForBlazorAsync();

        // Reload
        await Page.ReloadAsync();
        await _editPage.WaitForLoadAsync();
        await _editPage.SwitchToTablesAsync();
        await _editPage.Tables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Tables.IsTableVisibleAsync("T-100"), Is.True);
    }

    [Test]
    public async Task AddTable_ModalOpensInCreateMode()
    {
        // Act
        await _editPage.Tables.ClickAddTableAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Tables.EditTableModal.IsVisibleAsync(), Is.True);
            Assert.That(await _editPage.Tables.EditTableModal.IsCreateModeAsync(), Is.True);
            Assert.That(await _editPage.Tables.EditTableModal.IsDeleteButtonVisibleAsync(), Is.False);
        });

        // Cleanup
        await _editPage.Tables.EditTableModal.CancelAsync();
    }

    [Test]
    public async Task AddTable_CancelDoesNotCreateTable()
    {
        // Arrange
        var initialCount = await _editPage.Tables.GetTableCountAsync();

        // Act
        await _editPage.Tables.ClickAddTableAsync();
        await _editPage.Tables.EditTableModal.FillFormAsync(new TableFormData
        {
            TableNumber = "T-CANCEL",
            Capacity = 2,
            Location = "Test"
        });
        await _editPage.Tables.EditTableModal.CancelAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Tables.GetTableCountAsync();
        Assert.Multiple(() =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount));
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-CANCEL").Result, Is.False);
        });
    }

    #endregion

    #region Edit Table Tests

    [Test]
    public async Task EditTable_ModalOpensInEditMode()
    {
        // Arrange - ensure at least one table exists
        await EnsureTableExistsAsync("T-EDIT-MODE", 4, "Indoor");

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-EDIT-MODE");

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Tables.EditTableModal.IsVisibleAsync(), Is.True);
            Assert.That(await _editPage.Tables.EditTableModal.IsEditModeAsync(), Is.True);
            Assert.That(await _editPage.Tables.EditTableModal.IsDeleteButtonVisibleAsync(), Is.True);
        });

        // Cleanup
        await _editPage.Tables.EditTableModal.CancelAsync();
    }

    [Test]
    public async Task EditTable_UpdatesTableData()
    {
        // Arrange
        await EnsureTableExistsAsync("T-ORIGINAL", 4, "Indoor");

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-ORIGINAL");
        await _editPage.Tables.EditTableModal.FillFormAsync(new TableFormData
        {
            TableNumber = "T-UPDATED",
            Capacity = 8,
            Location = "VIP Area"
        });
        await _editPage.Tables.EditTableModal.SaveAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-UPDATED").Result, Is.True);
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-ORIGINAL").Result, Is.False);
        });
    }

    [Test]
    public async Task EditTable_PersistsChangesAfterReload()
    {
        // Arrange
        await EnsureTableExistsAsync("T-PERSIST", 2, "Bar");

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-PERSIST");
        await _editPage.Tables.EditTableModal.FillFormAsync(new TableFormData
        {
            TableNumber = "T-PERSISTED",
            Capacity = 10,
            Location = "Garden"
        });
        await _editPage.Tables.EditTableModal.SaveAsync();
        await WaitForBlazorAsync();

        // Reload
        await Page.ReloadAsync();
        await _editPage.WaitForLoadAsync();
        await _editPage.SwitchToTablesAsync();
        await _editPage.Tables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Tables.IsTableVisibleAsync("T-PERSISTED"), Is.True);
    }

    [Test]
    public async Task EditTable_CancelDoesNotSaveChanges()
    {
        // Arrange
        await EnsureTableExistsAsync("T-NOCHANGE", 4, "Indoor");

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-NOCHANGE");
        await _editPage.Tables.EditTableModal.FillFormAsync(new TableFormData
        {
            TableNumber = "T-CHANGED",
            Capacity = 99,
            Location = "Changed"
        });
        await _editPage.Tables.EditTableModal.CancelAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-NOCHANGE").Result, Is.True);
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-CHANGED").Result, Is.False);
        });
    }

    [Test]
    public async Task EditTable_LoadsCurrentValues()
    {
        // Arrange
        var expectedTable = new TableFormData
        {
            TableNumber = "T-VALUES",
            Capacity = 5,
            Location = "Patio"
        };
        await EnsureTableExistsAsync(expectedTable.TableNumber, expectedTable.Capacity.Value, expectedTable.Location);

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-VALUES");
        var currentValues = await _editPage.Tables.EditTableModal.GetCurrentValuesAsync();

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(currentValues.TableNumber, Is.EqualTo(expectedTable.TableNumber));
            Assert.That(currentValues.Capacity, Is.EqualTo(expectedTable.Capacity));
            Assert.That(currentValues.Location, Is.EqualTo(expectedTable.Location));
        });

        // Cleanup
        await _editPage.Tables.EditTableModal.CancelAsync();
    }

    #endregion

    #region Delete Table Tests

    [Test]
    public async Task DeleteTable_RemovesTableFromList()
    {
        // Arrange
        await EnsureTableExistsAsync("T-DELETE", 4, "Indoor");
        var initialCount = await _editPage.Tables.GetTableCountAsync();

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-DELETE");
        await _editPage.Tables.EditTableModal.DeleteAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Tables.GetTableCountAsync();
        Assert.Multiple(() =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount - 1));
            Assert.That(_editPage.Tables.IsTableVisibleAsync("T-DELETE").Result, Is.False);
        });
    }

    [Test]
    public async Task DeleteTable_PersistsAfterReload()
    {
        // Arrange
        await EnsureTableExistsAsync("T-DELETE-PERSIST", 4, "Indoor");

        // Act
        await _editPage.Tables.ClickTableByNameAsync("T-DELETE-PERSIST");
        await _editPage.Tables.EditTableModal.DeleteAsync();
        await WaitForBlazorAsync();

        // Reload
        await Page.ReloadAsync();
        await _editPage.WaitForLoadAsync();
        await _editPage.SwitchToTablesAsync();
        await _editPage.Tables.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Tables.IsTableVisibleAsync("T-DELETE-PERSIST"), Is.False);
    }

    #endregion

    #region Helper Methods

    private async Task EnsureTableExistsAsync(string tableNumber, int capacity, string location)
    {
        if (!await _editPage.Tables.IsTableVisibleAsync(tableNumber))
        {
            await _editPage.Tables.ClickAddTableAsync();
            await _editPage.Tables.EditTableModal.FillFormAsync(new TableFormData
            {
                TableNumber = tableNumber,
                Capacity = capacity,
                Location = location
            });
            await _editPage.Tables.EditTableModal.SaveAsync();
            await WaitForBlazorAsync();
        }
    }

    #endregion
}