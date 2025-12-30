using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

public class ManageMenuItemVariantsUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;
    private string _testCategoryName = null!;
    private string _testItemName = null!;

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

        Assert.That(await _editPage.Menu.NeedsMenuCreationAsync(), Is.False,
            "Menu should already exist for these tests");

        // Create test category
        _testCategoryName = $"VarCat{Guid.NewGuid():N}"[..12];
        await _editPage.Menu.AddCategoryAsync(_testCategoryName);
        await WaitForBlazorAsync();

        // Create test item
        _testItemName = $"VarItem{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData
            {
                Name = _testItemName,
                Description = "Test item for variant tests",
                Price = 20.00m,
                Currency = "PLN"
            },
            _testCategoryName);
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

    #region Add Variant Tests
    

    [Test]
    public async Task AddVariant_WithAllFields_Success()
    {
        // Arrange
        var variantName = $"Full{Guid.NewGuid():N}"[..10];
        var variantPrice = 25.50m;
        var variantDescription = "Full variant description";

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);

        // Act
        await item.AddVariantAsync(variantName, variantPrice, variantDescription);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await item.VariantExistsAsync(variantName), Is.True,
            "Variant with all fields should be created");
    }
    

    [Test]
    public async Task AddVariant_PersistsAfterRefresh()
    {
        // Arrange
        var variantName = $"Persist{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);

        // Act
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        item = _editPage.Menu.GetMenuItem(_testItemName);
        Assert.That(await item.VariantExistsAsync(variantName), Is.True,
            "Variant should persist after refresh");
    }

    [Test]
    public async Task AddMultipleVariants_ToSameItem_Success()
    {
        // Arrange
        var variant1Name = $"Var1{Guid.NewGuid():N}"[..10];
        var variant2Name = $"Var2{Guid.NewGuid():N}"[..10];
        var variant3Name = $"Var3{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        var initialCount = await item.GetVariantCountAsync();

        // Act
        await item.AddVariantAsync(variant1Name, 10.00m);
        await WaitForBlazorAsync();
        await item.AddVariantAsync(variant2Name, 15.00m);
        await WaitForBlazorAsync();
        await item.AddVariantAsync(variant3Name, 20.00m);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await item.GetVariantCountAsync();
        var variantNames = await item.GetVariantNamesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount + 3), "Should have 3 more variants");
            Assert.That(variantNames, Does.Contain(variant1Name), "First variant should exist");
            Assert.That(variantNames, Does.Contain(variant2Name), "Second variant should exist");
            Assert.That(variantNames, Does.Contain(variant3Name), "Third variant should exist");
        });
    }

    [Test]
    public async Task AddVariant_AppearsInVariantList()
    {
        // Arrange
        var variantName = $"List{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);

        // Act
        await item.AddVariantAsync(variantName, 12.50m, "Test description");
        await WaitForBlazorAsync();

        // Assert
        var variantNames = await item.GetVariantNamesAsync();
        Assert.That(variantNames, Does.Contain(variantName),
            "Variant should appear in variant names list");
    }

    #endregion

    #region Edit Variant Tests

    [Test]
    public async Task EditVariant_ChangeName_Success()
    {
        // Arrange
        var originalName = $"Orig{Guid.NewGuid():N}"[..10];
        var newName = $"New{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(originalName, 10.00m);
        await WaitForBlazorAsync();

        // Act
        await item.EditVariantAsync(originalName, newName: newName);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await item.VariantExistsAsync(newName), Is.True, "Renamed variant should exist");
            Assert.That(await item.VariantExistsAsync(originalName), Is.False, "Original name should not exist");
        });
    }

    [Test]
    public async Task EditVariant_ChangePrice_Success()
    {
        // Arrange
        var variantName = $"Price{Guid.NewGuid():N}"[..10];
        var originalPrice = 10.00m;
        var newPrice = 99.99m;

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, originalPrice);
        await WaitForBlazorAsync();

        // Act
        await item.EditVariantAsync(variantName, newPrice: newPrice);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await item.VariantExistsAsync(variantName), Is.True,
            "Variant should still exist after price change");
    }

    [Test]
    public async Task EditVariant_ChangeDescription_Success()
    {
        // Arrange
        var variantName = $"Desc{Guid.NewGuid():N}"[..10];
        var originalDescription = "Original description";
        var newDescription = "Updated description text";

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m, originalDescription);
        await WaitForBlazorAsync();

        // Act
        await item.EditVariantAsync(variantName, newDescription: newDescription);
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await item.VariantExistsAsync(variantName), Is.True,
            "Variant should still exist after description change");
    }

    [Test]
    public async Task EditVariant_ChangesPersistAfterRefresh()
    {
        // Arrange
        var originalName = $"Pers{Guid.NewGuid():N}"[..10];
        var newName = $"Edited{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(originalName, 10.00m);
        await WaitForBlazorAsync();

        // Ensure variants are visible before edit
        await item.ShowVariantsAsync();
        await WaitForBlazorAsync();

        // Act
        await item.EditVariantAsync(originalName, newName: newName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        item = _editPage.Menu.GetMenuItem(_testItemName);
        Assert.Multiple(async () =>
        {
            Assert.That(await item.VariantExistsAsync(newName), Is.True, "Edited variant should persist");
            Assert.That(await item.VariantExistsAsync(originalName), Is.False, "Original name should not reappear");
        });
    }
    #endregion

    #region Delete Variant Tests

    [Test]
    public async Task DeleteVariant_RemovesFromItem()
    {
        // Arrange
        var variantName = $"Del{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        var initialCount = await item.GetVariantCountAsync();

        // Act
        await item.DeleteVariantAsync(variantName);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await item.GetVariantCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount - 1), "Variant count should decrease by 1");
            Assert.That(await item.VariantExistsAsync(variantName), Is.False, "Deleted variant should not exist");
        });
    }

    [Test]
    public async Task DeleteVariant_VariantNoLongerInList()
    {
        // Arrange
        var variantName = $"Gone{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        // Act
        await item.DeleteVariantAsync(variantName);
        await WaitForBlazorAsync();

        // Assert
        var variantNames = await item.GetVariantNamesAsync();
        Assert.That(variantNames, Does.Not.Contain(variantName),
            "Deleted variant should not appear in list");
    }

    [Test]
    public async Task DeleteVariant_PersistsAfterRefresh()
    {
        // Arrange
        var variantName = $"DelPer{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        // Act
        await item.DeleteVariantAsync(variantName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        item = _editPage.Menu.GetMenuItem(_testItemName);
        Assert.That(await item.VariantExistsAsync(variantName), Is.False,
            "Deleted variant should not reappear after refresh");
    }

    [Test]
    public async Task DeleteVariant_OtherVariantsRemain()
    {
        // Arrange
        var variant1Name = $"Del1{Guid.NewGuid():N}"[..10];
        var variant2Name = $"Keep{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variant1Name, 10.00m);
        await WaitForBlazorAsync();
        await item.AddVariantAsync(variant2Name, 20.00m);
        await WaitForBlazorAsync();

        // Act
        await item.DeleteVariantAsync(variant1Name);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await item.VariantExistsAsync(variant1Name), Is.False, "Deleted variant should not exist");
            Assert.That(await item.VariantExistsAsync(variant2Name), Is.True, "Other variant should remain");
        });
    }
    

    [Test]
    public async Task DeleteAllVariants_ItemHasNoVariants()
    {
        // Arrange
        var variantName = $"Last{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        // Act
        await item.DeleteVariantAsync(variantName);
        await WaitForBlazorAsync();

        // Assert
        var variantCount = await item.GetVariantCountAsync();
        Assert.That(variantCount, Is.EqualTo(0), "Item should have no variants");
    }

    #endregion

    #region Variants Visibility Tests

    [Test]
    public async Task ShowVariants_MakesVariantsSectionVisible()
    {
        // Arrange
        var variantName = $"Show{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        // First hide variants
        await item.HideVariantsAsync();
        await WaitForBlazorAsync();

        // Act
        await item.ShowVariantsAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await item.AreVariantsVisibleAsync(), Is.True,
            "Variants section should be visible");
    }

    [Test]
    public async Task HideVariants_HidesVariantsSection()
    {
        // Arrange
        var variantName = $"Hide{Guid.NewGuid():N}"[..10];

        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddVariantAsync(variantName, 10.00m);
        await WaitForBlazorAsync();

        // Ensure variants are shown first
        await item.ShowVariantsAsync();
        await WaitForBlazorAsync();

        // Act
        await item.HideVariantsAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await item.AreVariantsVisibleAsync(), Is.False,
            "Variants section should be hidden");
    }

    #endregion

}