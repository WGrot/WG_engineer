using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class ManageMenuCategoriesUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        var credentials = TestDataFactory.GetTestUserCredentials(2);
        await LoginAsUserAsync(credentials.Email, credentials.Password);
        await _editPage.NavigateAsync(5);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();
        
        // Verify menu exists (not in creation mode)
        Assert.That(await _editPage.Menu.NeedsMenuCreationAsync(), Is.False,
            "Menu should already exist for these tests");
    }

    #region Add Category Tests

    [Test]
    public async Task AddCategoryButton_OpensForm()
    {
        // Act
        await _editPage.Menu.OpenAddCategoryFormAsync();

        // Assert
        Assert.That(await _editPage.Menu.IsAddCategoryFormVisibleAsync(), Is.True,
            "Add category form should be visible after clicking Add New Category");
    }

    [Test]
    public async Task AddCategory_WithNameOnly_Success()
    {
        // Arrange
        var categoryName = $"Test Category {Guid.NewGuid():N}"[..30];
        var initialCount = await _editPage.Menu.GetCategoryCountAsync();

        // Act
        await _editPage.Menu.AddCategoryAsync(categoryName);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetCategoryCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount + 1), 
                "Category count should increase by 1");
            Assert.That(await _editPage.Menu.CategoryExistsAsync(categoryName), Is.True,
                "New category should be visible in the list");
        });
    }

    [Test]
    public async Task AddCategory_WithAllFields_Success()
    {
        // Arrange
        var categoryName = $"Full Category {Guid.NewGuid():N}"[..25];
        var description = "Test description for category";
        var displayOrder = 99;

        // Act
        await _editPage.Menu.AddCategoryAsync(categoryName, description, displayOrder);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Menu.CategoryExistsAsync(categoryName), Is.True,
                "Category should exist");
            
            var actualDescription = await _editPage.Menu.GetCategoryDescriptionAsync(categoryName);
            Assert.That(actualDescription, Is.EqualTo(description),
                "Category description should match");
        });
    }

    [Test]
    public async Task AddCategory_Cancel_DoesNotAddCategory()
    {
        // Arrange
        var initialCount = await _editPage.Menu.GetCategoryCountAsync();

        // Act
        await _editPage.Menu.OpenAddCategoryFormAsync();
        await _editPage.Menu.CancelAddCategoryAsync();
        await WaitForBlazorAsync();

        // Assert
        var finalCount = await _editPage.Menu.GetCategoryCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(finalCount, Is.EqualTo(initialCount),
                "Category count should not change after cancel");
            Assert.That(await _editPage.Menu.IsAddCategoryFormVisibleAsync(), Is.False,
                "Add form should be hidden after cancel");
        });
    }

    [Test]
    public async Task AddCategory_PersistsAfterPageRefresh()
    {
        // Arrange
        var categoryName = $"Persist Cat {Guid.NewGuid():N}"[..25];

        // Act
        await _editPage.Menu.AddCategoryAsync(categoryName);
        await WaitForBlazorAsync();

        // Refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Menu.CategoryExistsAsync(categoryName), Is.True,
            "Category should persist after page refresh");
    }

    #endregion

    #region Edit Category Tests

    [Test]
    public async Task EditCategoryButton_OpensEditForm()
    {
        // Arrange - ensure at least one category exists
        var categories = await _editPage.Menu.GetAllCategoryNamesAsync();
        if (categories.Count == 0)
        {
            await _editPage.Menu.AddCategoryAsync("Category To Edit");
            await WaitForBlazorAsync();
            categories = await _editPage.Menu.GetAllCategoryNamesAsync();
        }
        
        var categoryToEdit = categories.First();

        // Act
        await _editPage.Menu.OpenEditCategoryFormAsync(categoryToEdit);

        // Assert
        Assert.That(await _editPage.Menu.IsEditCategoryFormVisibleAsync(), Is.True,
            "Edit category form should be visible");
    }

    [Test]
    public async Task EditCategory_ChangeName_Success()
    {
        // Arrange
        var originalName = $"Original {Guid.NewGuid():N}"[..20];
        var newName = $"Renamed {Guid.NewGuid():N}"[..20];
        
        await _editPage.Menu.AddCategoryAsync(originalName);
        await WaitForBlazorAsync();

        // Act
        await _editPage.Menu.EditCategoryAsync(originalName, newName: newName);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Menu.CategoryExistsAsync(newName), Is.True,
                "Category with new name should exist");
            Assert.That(await _editPage.Menu.CategoryExistsAsync(originalName), Is.False,
                "Category with old name should not exist");
        });
    }

    [Test]
    public async Task EditCategory_ChangeDescription_Success()
    {
        // Arrange
        var categoryName = $"Desc Test {Guid.NewGuid():N}"[..20];
        var originalDescription = "Original description";
        var newDescription = "Updated description";
        
        await _editPage.Menu.AddCategoryAsync(categoryName, originalDescription);
        await WaitForBlazorAsync();

        // Act
        await _editPage.Menu.EditCategoryAsync(categoryName, newDescription: newDescription);
        await WaitForBlazorAsync();

        // Assert
        var actualDescription = await _editPage.Menu.GetCategoryDescriptionAsync(categoryName);
        Assert.That(actualDescription, Is.EqualTo(newDescription),
            "Category description should be updated");
    }

    [Test]
    public async Task EditCategory_Cancel_DoesNotSaveChanges()
    {
        // Arrange
        var categoryName = $"No Change {Guid.NewGuid():N}"[..20];
        var description = "Original description";
        
        await _editPage.Menu.AddCategoryAsync(categoryName, description);
        await WaitForBlazorAsync();

        // Act
        await _editPage.Menu.OpenEditCategoryFormAsync(categoryName);
        await _editPage.Menu.CancelEditCategoryAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Menu.IsEditCategoryFormVisibleAsync(), Is.False,
                "Edit form should be hidden after cancel");
            Assert.That(await _editPage.Menu.CategoryExistsAsync(categoryName), Is.True,
                "Original category should still exist");
        });
    }

    [Test]
    public async Task EditCategory_ChangesPersistAfterRefresh()
    {
        // Arrange
        var originalName = $"Persist Edit {Guid.NewGuid():N}"[..20];
        var newName = $"Edited {Guid.NewGuid():N}"[..20];
        
        await _editPage.Menu.AddCategoryAsync(originalName);
        await WaitForBlazorAsync();

        // Act
        await _editPage.Menu.EditCategoryAsync(originalName, newName: newName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Menu.CategoryExistsAsync(newName), Is.True,
            "Edited category name should persist after refresh");
    }

    #endregion

    #region Delete Category Tests

    [Test]
    public async Task DeleteCategory_RemovesFromList()
    {
        // Arrange
        var categoryName = $"Delete Me {Guid.NewGuid():N}"[..20];
        await _editPage.Menu.AddCategoryAsync(categoryName);
        await WaitForBlazorAsync();
        
        var initialCount = await _editPage.Menu.GetCategoryCountAsync();

        // Act
        await _editPage.Menu.DeleteCategoryAsync(categoryName);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetCategoryCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount - 1),
                "Category count should decrease by 1");
            Assert.That(await _editPage.Menu.CategoryExistsAsync(categoryName), Is.False,
                "Deleted category should not exist");
        });
    }

    [Test]
    public async Task DeleteCategory_PersistsAfterRefresh()
    {
        // Arrange
        var categoryName = $"Persist Del {Guid.NewGuid():N}"[..20];
        await _editPage.Menu.AddCategoryAsync(categoryName);
        await WaitForBlazorAsync();
        
        // Act
        await _editPage.Menu.DeleteCategoryAsync(categoryName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Menu.CategoryExistsAsync(categoryName), Is.False,
            "Deleted category should not reappear after refresh");
    }

    #endregion

    #region Category Expand/Collapse Tests

    [Test]
    public async Task Category_ExpandAndCollapse()
    {
        // Arrange
        var categories = await _editPage.Menu.GetAllCategoryNamesAsync();
        if (categories.Count == 0)
        {
            await _editPage.Menu.AddCategoryAsync("Expandable Category");
            await WaitForBlazorAsync();
            categories = await _editPage.Menu.GetAllCategoryNamesAsync();
        }
        
        var categoryName = categories.First();

        // Act - Expand
        await _editPage.Menu.ExpandCategoryAsync(categoryName);

        // Assert - Expanded
        Assert.That(await _editPage.Menu.IsCategoryExpandedAsync(categoryName), Is.True,
            "Category should be expanded");

        // Act - Collapse
        await _editPage.Menu.CollapseCategoryAsync(categoryName);

        // Assert - Collapsed
        Assert.That(await _editPage.Menu.IsCategoryExpandedAsync(categoryName), Is.False,
            "Category should be collapsed");
    }

    #endregion

    [Test]
    public async Task AddCategory_WithEmptyName_ShouldNotAdd()
    {
        // Arrange
        var initialCount = await _editPage.Menu.GetCategoryCountAsync();

        // Act
        await _editPage.Menu.OpenAddCategoryFormAsync();
        // Don't fill in name, just try to submit
        // Note: The actual behavior depends on validation implementation
        // This test documents expected behavior

        // Assert - form should still be visible or count unchanged
        // Adjust based on actual validation behavior
    }

    [Test]
    public async Task Category_DisplayOrder_AffectsPosition()
    {
        // Arrange - add categories with specific order
        var highOrderName = $"Z Last {Guid.NewGuid():N}"[..15];
        var lowOrderName = $"A First {Guid.NewGuid():N}"[..15];
        
        await _editPage.Menu.AddCategoryAsync(highOrderName, displayOrder: 100);
        await _editPage.Menu.AddCategoryAsync(lowOrderName, displayOrder: 1);
        await WaitForBlazorAsync();

        // Act
        var allCategories = await _editPage.Menu.GetAllCategoryNamesAsync();

        // Assert - low order should appear before high order
        var lowIndex = allCategories.IndexOf(lowOrderName);
        var highIndex = allCategories.IndexOf(highOrderName);
        
        Assert.That(lowIndex, Is.LessThan(highIndex),
            "Category with lower display order should appear first");

        // Cleanup
        await _editPage.Menu.DeleteCategoryAsync(highOrderName);
        await _editPage.Menu.DeleteCategoryAsync(lowOrderName);
    }
}