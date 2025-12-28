using RestaurantApp.E2ETests.PageObjects;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

public class AddTagToMenuItemUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;
    private string _testCategoryName = null!;
    private string _testTagName = null!;
    private string _testItemName = null!;

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

        // Create test category
        _testCategoryName = $"TagCat{Guid.NewGuid():N}"[..12];
        await _editPage.Menu.AddCategoryAsync(_testCategoryName);
        await WaitForBlazorAsync();

        // Create test tag
        _testTagName = $"Tag{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddTagAsync(_testTagName, "#3498db");
        await WaitForBlazorAsync();

        // Create test item
        _testItemName = $"Item{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddItemAsync(
            new MenuItemFormData
            {
                Name = _testItemName,
                Description = "Test item for tag tests",
                Price = 15.00m,
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

            if (await _editPage.Menu.TagExistsAsync(_testTagName))
            {
                await _editPage.Menu.DeleteTagAsync(_testTagName);
            }
        }
        catch { /* Ignore cleanup errors */ }
    }
    

    [Test]
    public async Task AddTag_ToMenuItem_Success()
    {
        // Arrange
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        var initialTagCount = await item.GetTagCountAsync();

        // Act
        await item.AddTagAsync(_testTagName);
        await WaitForBlazorAsync();

        // Assert
        var newTagCount = await item.GetTagCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newTagCount, Is.EqualTo(initialTagCount + 1), "Tag count should increase by 1");
            Assert.That(await item.HasTagAsync(_testTagName), Is.True, "Item should have the added tag");
        });
    }
    

    [Test]
    public async Task AddMultipleTags_ToSameItem_Success()
    {
        // Arrange - create second tag
        var secondTagName = $"Tag2{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddTagAsync(secondTagName, "#e74c3c");
        await WaitForBlazorAsync();

        try
        {
            await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
            var item = _editPage.Menu.GetMenuItem(_testItemName);

            // Act
            await item.AddTagAsync(_testTagName);
            await WaitForBlazorAsync();
            await item.AddTagAsync(secondTagName);
            await WaitForBlazorAsync();

            // Assert
            var tags = await item.GetTagsAsync();
            Assert.Multiple(() =>
            {
                Assert.That(tags, Does.Contain(_testTagName), "First tag should be present");
                Assert.That(tags, Does.Contain(secondTagName), "Second tag should be present");
                Assert.That(tags.Count, Is.GreaterThanOrEqualTo(2), "Item should have at least 2 tags");
            });
        }
        finally
        {
            // Cleanup second tag
            if (await _editPage.Menu.TagExistsAsync(secondTagName))
            {
                await _editPage.Menu.DeleteTagAsync(secondTagName);
            }
        }
    }

    [Test]
    public async Task AddTag_CancelOperation_DoesNotAddTag()
    {
        // Arrange
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        var initialTagCount = await item.GetTagCountAsync();

        // Act
        await item.OpenAddTagDropdownAsync();
        await item.CancelAddTagAsync();
        await WaitForBlazorAsync();

        // Assert
        var newTagCount = await item.GetTagCountAsync();
        Assert.That(newTagCount, Is.EqualTo(initialTagCount),
            "Tag count should remain unchanged after cancel");
    }
    

    [Test]
    public async Task RemoveTag_FromMenuItem_Success()
    {
        // Arrange - first add a tag
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddTagAsync(_testTagName);
        await WaitForBlazorAsync();

        var initialTagCount = await item.GetTagCountAsync();

        // Act
        await item.RemoveTagAsync(_testTagName);
        await WaitForBlazorAsync();

        // Assert
        var newTagCount = await item.GetTagCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newTagCount, Is.EqualTo(initialTagCount - 1), "Tag count should decrease by 1");
            Assert.That(await item.HasTagAsync(_testTagName), Is.False, "Item should not have the removed tag");
        });
    }
    

    [Test]
    public async Task RemoveOneTag_OtherTagsRemain()
    {
        // Arrange - create and add two tags
        var secondTagName = $"Tag2{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddTagAsync(secondTagName, "#9b59b6");
        await WaitForBlazorAsync();

        try
        {
            await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
            var item = _editPage.Menu.GetMenuItem(_testItemName);

            await item.AddTagAsync(_testTagName);
            await WaitForBlazorAsync();
            await item.AddTagAsync(secondTagName);
            await WaitForBlazorAsync();

            // Act - remove only the first tag
            await item.RemoveTagAsync(_testTagName);
            await WaitForBlazorAsync();

            // Assert
            Assert.Multiple(async () =>
            {
                Assert.That(await item.HasTagAsync(_testTagName), Is.False, "Removed tag should not be present");
                Assert.That(await item.HasTagAsync(secondTagName), Is.True, "Other tag should still be present");
            });
        }
        finally
        {
            // Cleanup second tag
            if (await _editPage.Menu.TagExistsAsync(secondTagName))
            {
                await _editPage.Menu.DeleteTagAsync(secondTagName);
            }
        }
    }

    [Test]
    public async Task RemoveAllTags_ItemHasNoTags()
    {
        // Arrange - add a tag first
        await _editPage.Menu.ExpandCategoryAsync(_testCategoryName);
        var item = _editPage.Menu.GetMenuItem(_testItemName);
        await item.AddTagAsync(_testTagName);
        await WaitForBlazorAsync();

        // Act
        await item.RemoveTagAsync(_testTagName);
        await WaitForBlazorAsync();

        // Assert
        var tagCount = await item.GetTagCountAsync();
        Assert.That(tagCount, Is.EqualTo(0), "Item should have no tags");
    }

}