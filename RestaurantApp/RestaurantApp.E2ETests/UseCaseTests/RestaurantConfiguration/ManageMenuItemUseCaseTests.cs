using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.MenuManagement;

[TestFixture]
public class ManageMenuItemUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;
    private string _testCategoryName = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        Assert.That(await _editPage.Menu.NeedsMenuCreationAsync(), Is.False,
            "Menu should already exist for these tests");
        
        _testCategoryName = $"TestCat{Guid.NewGuid():N}"[..15];
        await _editPage.Menu.AddCategoryAsync(_testCategoryName);
        await WaitForBlazorAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {

        try
        {
            await _editPage.NavigateAsync(1);
            await _editPage.SwitchToMenuAsync();
            await _editPage.Menu.WaitForLoadAsync();
            await WaitForBlazorAsync();

            if (await _editPage.Menu.CategoryExistsAsync(_testCategoryName))
            {
                await _editPage.Menu.DeleteCategoryAsync(_testCategoryName);
            }
        }
        catch { /* Ignore cleanup errors */ }
    }

    #region Add Item Tests

    [Test]
    public async Task AddItem_ToCategory_Success()
    {
        // Arrange
        var itemName = $"Item{Guid.NewGuid():N}"[..12];
        var initialCount = await _editPage.Menu.GetItemCountInCategoryAsync(_testCategoryName);

        // Act
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData
            {
                Name = itemName,
                Description = "Test description",
                Price = 19.99m,
                Currency = "PLN"
            },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetItemCountInCategoryAsync(_testCategoryName);
        Assert.That(newCount, Is.EqualTo(initialCount + 1),
            "Item count should increase by 1");
    }

    [Test]
    public async Task AddItem_ToUncategorized_Success()
    {
        // Arrange
        var itemName = $"Uncat{Guid.NewGuid():N}"[..12];
        var initialCount = await _editPage.Menu.GetUncategorizedItemCountAsync();

        // Act
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData
            {
                Name = itemName,
                Price = 9.99m,
                Currency = "PLN"
            },
            categoryName: null);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetUncategorizedItemCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount + 1),
            "Uncategorized item count should increase by 1");
    }

    [Test]
    public async Task AddItem_WithAllFields_DisplaysCorrectly()
    {
        // Arrange
        var itemName = $"Full{Guid.NewGuid():N}"[..10];
        var description = "Full item description";
        var price = 25.50m;

        // Act
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData
            {
                Name = itemName,
                Description = description,
                Price = price,
                Currency = "PLN"
            },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Assert - expand category to see item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        
        Assert.Multiple(async () =>
        {
            Assert.That(await item.IsVisibleAsync(), Is.True, "Item should be visible");
            Assert.That(await item.GetNameAsync(), Is.EqualTo(itemName), "Name should match");
            Assert.That(await item.GetPriceDisplayAsync(), Does.Contain("25.5"), "Price should be displayed");
        });
    }

    [Test]
    public async Task AddItem_PersistsAfterRefresh()
    {
        // Arrange
        var itemName = $"Persist{Guid.NewGuid():N}"[..12];

        // Act
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 15.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert - expand category first
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        Assert.That(await item.IsVisibleAsync(), Is.True,
            "Item should persist after refresh");
    }

    #endregion

    #region Edit Item Tests

    [Test]
    public async Task EditItem_ChangeName_Success()
    {
        // Arrange
        var originalName = $"Orig{Guid.NewGuid():N}"[..10];
        var newName = $"New{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = originalName, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(originalName);
        await item.EditAsync(name: newName);
        await WaitForBlazorAsync();

        // Assert - expand again after edit (page might refresh)
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var renamedItem = _editPage.Menu.GetMenuItem(newName);
        var originalItem = _editPage.Menu.GetMenuItem(originalName);

        Assert.Multiple(async () =>
        {
            Assert.That(await renamedItem.IsVisibleAsync(), Is.True, "Renamed item should exist");
            Assert.That(await originalItem.IsVisibleAsync(), Is.False, "Original name should not exist");
        });
    }

    [Test]
    public async Task EditItem_ChangePrice_Success()
    {
        // Arrange
        var itemName = $"Price{Guid.NewGuid():N}"[..10];
        var newPrice = 99.99m;

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.EditAsync(price: newPrice);
        await WaitForBlazorAsync();

        // Assert - expand again and get fresh reference
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        item = _editPage.Menu.GetMenuItem(itemName);
        var priceDisplay = await item.GetPriceDisplayAsync();
        Assert.That(priceDisplay, Does.Contain("99.99"),
            "Price should be updated");
    }

    [Test]
    public async Task EditItem_ChangeDescription_Success()
    {
        // Arrange
        var itemName = $"Desc{Guid.NewGuid():N}"[..10];
        var newDescription = "Updated description text";

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Description = "Original", Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.EditAsync(description: newDescription);
        await WaitForBlazorAsync();

        // Assert - expand again and get fresh reference
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        item = _editPage.Menu.GetMenuItem(itemName);
        var description = await item.GetDescriptionAsync();
        Assert.That(description, Is.EqualTo(newDescription),
            "Description should be updated");
    }

    [Test]
    public async Task EditItem_Cancel_DoesNotSaveChanges()
    {
        // Arrange
        var itemName = $"Cancel{Guid.NewGuid():N}"[..10];
        var originalPrice = 10.00m;

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = originalPrice, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.StartEditAsync();
        await item.FillEditFormAsync(price: 999.99m);
        await item.CancelEditAsync();
        await WaitForBlazorAsync();

        // Assert
        var priceDisplay = await item.GetPriceDisplayAsync();
        Assert.That(priceDisplay, Does.Contain("10"),
            "Price should remain unchanged after cancel");
    }

    [Test]
    public async Task EditItem_ChangesPersistAfterRefresh()
    {
        // Arrange
        var itemName = $"EditPer{Guid.NewGuid():N}"[..10];
        var newName = $"Edited{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.EditAsync(name: newName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        await WaitForBlazorAsync();

        // Assert
        var editedItem = _editPage.Menu.GetMenuItem(newName);
        Assert.That(await editedItem.IsVisibleAsync(), Is.True,
            "Edited item should persist after refresh");
    }

    #endregion

    #region Delete Item Tests

    [Test]
    public async Task DeleteItem_RemovesFromCategory()
    {
        // Arrange
        var itemName = $"Del{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        var initialCount = await _editPage.Menu.GetItemCountInCategoryAsync(_testCategoryName);

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.DeleteAsync();
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetItemCountInCategoryAsync(_testCategoryName);
        Assert.That(newCount, Is.EqualTo(initialCount - 1),
            "Item count should decrease by 1");
    }

    [Test]
    public async Task DeleteItem_ItemNoLongerVisible()
    {
        // Arrange
        var itemName = $"Gone{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.DeleteAsync();
        await WaitForBlazorAsync();

        // Assert - expand again to verify item is gone
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        item = _editPage.Menu.GetMenuItem(itemName);
        Assert.That(await item.IsVisibleAsync(), Is.False,
            "Deleted item should not be visible");
    }

    [Test]
    public async Task DeleteItem_PersistsAfterRefresh()
    {
        // Arrange
        var itemName = $"DelPer{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category before interacting with item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.DeleteAsync();
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        await WaitForBlazorAsync();

        // Assert
        var deletedItem = _editPage.Menu.GetMenuItem(itemName);
        Assert.That(await deletedItem.IsVisibleAsync(), Is.False,
            "Deleted item should not reappear after refresh");
    }

    [Test]
    public async Task DeleteMultipleItems_AllRemoved()
    {
        // Arrange
        var item1Name = $"Multi1{Guid.NewGuid():N}"[..10];
        var item2Name = $"Multi2{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = item1Name, Price = 10.00m, Currency = "PLN" },
            _testCategoryName);
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = item2Name, Price = 20.00m, Currency = "PLN" },
            _testCategoryName);
        await WaitForBlazorAsync();

        // Act - expand category and delete first item
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item1 = _editPage.Menu.GetMenuItem(item1Name);
        await item1.DeleteAsync();
        await WaitForBlazorAsync();

        // After delete, page refreshes - need to expand again and get fresh locator
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item2 = _editPage.Menu.GetMenuItem(item2Name);
        await item2.DeleteAsync();
        await WaitForBlazorAsync();

        // Assert - expand again to verify both are gone
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        
        var deletedItem1 = _editPage.Menu.GetMenuItem(item1Name);
        var deletedItem2 = _editPage.Menu.GetMenuItem(item2Name);
        
        Assert.Multiple(async () =>
        {
            Assert.That(await deletedItem1.IsVisibleAsync(), Is.False, "First item should be deleted");
            Assert.That(await deletedItem2.IsVisibleAsync(), Is.False, "Second item should be deleted");
        });
    }

    #endregion
}


[TestFixture]
public class MenuItemMoveUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;
    private string _sourceCategoryName = null!;
    private string _targetCategoryName = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Create two test categories
        _sourceCategoryName = $"Source{Guid.NewGuid():N}"[..12];
        _targetCategoryName = $"Target{Guid.NewGuid():N}"[..12];

        await _editPage.Menu.AddCategoryAsync(_sourceCategoryName);
        await _editPage.Menu.AddCategoryAsync(_targetCategoryName);
        await WaitForBlazorAsync();
    }

    [TearDown]
    public async Task Cleanup()
    {
        try
        {
            // Refresh to get clean state
            await _editPage.NavigateAsync(1);
            await _editPage.SwitchToMenuAsync();
            await _editPage.Menu.WaitForLoadAsync();
            await WaitForBlazorAsync();

            if (await _editPage.Menu.CategoryExistsAsync(_sourceCategoryName))
                await _editPage.Menu.DeleteCategoryAsync(_sourceCategoryName);
            if (await _editPage.Menu.CategoryExistsAsync(_targetCategoryName))
                await _editPage.Menu.DeleteCategoryAsync(_targetCategoryName);
        }
        catch
        {
            // ignored
        }
    }

    [Test]
    public async Task MoveItem_BetweenCategories_Success()
    {
        // Arrange
        var itemName = $"Move{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _sourceCategoryName);
        await WaitForBlazorAsync();

        var sourceCount = await _editPage.Menu.GetItemCountInCategoryAsync(_sourceCategoryName);
        var targetCount = await _editPage.Menu.GetItemCountInCategoryAsync(_targetCategoryName);

        // Act - expand category and move item
        await _editPage.Menu.ExpandCategoryAsync(_sourceCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.MoveToCategoryAsync(_targetCategoryName);
        await WaitForBlazorAsync();

        // Refresh to see changes
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var newSourceCount = await _editPage.Menu.GetItemCountInCategoryAsync(_sourceCategoryName);
        var newTargetCount = await _editPage.Menu.GetItemCountInCategoryAsync(_targetCategoryName);

        Assert.Multiple(() =>
        {
            Assert.That(newSourceCount, Is.EqualTo(sourceCount - 1), "Source should have one less item");
            Assert.That(newTargetCount, Is.EqualTo(targetCount + 1), "Target should have one more item");
        });
    }

    [Test]
    public async Task MoveItem_ToUncategorized_Success()
    {
        // Arrange
        var itemName = $"ToUncat{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            _sourceCategoryName);
        await WaitForBlazorAsync();

        var uncatCount = await _editPage.Menu.GetUncategorizedItemCountAsync();

        // Act - expand category and move item
        await _editPage.Menu.ExpandCategoryAsync(_sourceCategoryName);
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.MoveToUncategorizedAsync();
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var newUncatCount = await _editPage.Menu.GetUncategorizedItemCountAsync();
        Assert.That(newUncatCount, Is.EqualTo(uncatCount + 1),
            "Uncategorized should have one more item");
    }

    [Test]
    public async Task MoveItem_FromUncategorizedToCategory_Success()
    {
        // Arrange
        var itemName = $"FromUncat{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData { Name = itemName, Price = 10.00m, Currency = "PLN" },
            categoryName: null); // Add to uncategorized
        await WaitForBlazorAsync();

        var targetCount = await _editPage.Menu.GetItemCountInCategoryAsync(_targetCategoryName);

        // Act - expand uncategorized and move item
        await _editPage.Menu.ExpandUncategorizedAsync();
        var item = _editPage.Menu.GetMenuItem(itemName);
        await item.MoveToCategoryAsync(_targetCategoryName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var newTargetCount = await _editPage.Menu.GetItemCountInCategoryAsync(_targetCategoryName);
        Assert.That(newTargetCount, Is.EqualTo(targetCount + 1),
            "Target category should have one more item");
    }
}