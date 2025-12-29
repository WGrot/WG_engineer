using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class ManageMenuTagsUseCaseTests: PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;

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
    }

    #region Add Tag Tests

    [Test]
    public async Task AddTagButton_OpensForm()
    {
        // Act
        await _editPage.Menu.OpenAddTagFormAsync();

        // Assert
        Assert.That(await _editPage.Menu.IsAddTagFormVisibleAsync(), Is.True,
            "Add tag form should be visible");
    }

    [Test]
    public async Task AddTag_WithNameAndColor_Success()
    {
        // Arrange
        var tagName = $"Tag{Guid.NewGuid():N}"[..10];
        var tagColor = "#FF5733";
        var initialCount = await _editPage.Menu.GetTagCountAsync();

        // Act
        await _editPage.Menu.AddTagAsync(tagName, tagColor);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetTagCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount + 1), "Tag count should increase by 1");
            Assert.That(await _editPage.Menu.TagExistsAsync(tagName), Is.True, "New tag should be visible");
        });
    }

    [Test]
    public async Task AddTag_Cancel_DoesNotAddTag()
    {
        // Arrange
        var initialCount = await _editPage.Menu.GetTagCountAsync();

        // Act
        await _editPage.Menu.OpenAddTagFormAsync();
        await _editPage.Menu.CancelAddTagAsync();
        await WaitForBlazorAsync();

        // Assert
        var finalCount = await _editPage.Menu.GetTagCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(finalCount, Is.EqualTo(initialCount), "Tag count should not change");
            Assert.That(await _editPage.Menu.IsAddTagFormVisibleAsync(), Is.False, "Form should be hidden");
        });
    }

    [Test]
    public async Task AddTag_PersistsAfterRefresh()
    {
        // Arrange
        var tagName = $"Persist{Guid.NewGuid():N}"[..10];

        // Act
        await _editPage.Menu.AddTagAsync(tagName, "#00FF00");
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Menu.TagExistsAsync(tagName), Is.True,
            "Tag should persist after refresh");
    }

    [Test]
    public async Task AddMultipleTags_AllVisible()
    {
        // Arrange
        var tag1 = $"Multi1{Guid.NewGuid():N}"[..8];
        var tag2 = $"Multi2{Guid.NewGuid():N}"[..8];
        var initialCount = await _editPage.Menu.GetTagCountAsync();

        // Act
        await _editPage.Menu.AddTagAsync(tag1, "#FF0000");
        await _editPage.Menu.AddTagAsync(tag2, "#0000FF");
        await WaitForBlazorAsync();

        // Assert
        var finalCount = await _editPage.Menu.GetTagCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(finalCount, Is.EqualTo(initialCount + 2), "Two tags should be added");
            Assert.That(await _editPage.Menu.TagExistsAsync(tag1), Is.True);
            Assert.That(await _editPage.Menu.TagExistsAsync(tag2), Is.True);
        });
    }

    #endregion

    #region Delete Tag Tests

    [Test]
    public async Task DeleteTag_RemovesFromList()
    {
        // Arrange
        var tagName = $"Del{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddTagAsync(tagName, "#FF0000");
        await WaitForBlazorAsync();

        var initialCount = await _editPage.Menu.GetTagCountAsync();

        // Act
        await _editPage.Menu.DeleteTagAsync(tagName);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Menu.GetTagCountAsync();
        Assert.Multiple(async () =>
        {
            Assert.That(newCount, Is.EqualTo(initialCount - 1), "Tag count should decrease by 1");
            Assert.That(await _editPage.Menu.TagExistsAsync(tagName), Is.False, "Tag should not exist");
        });
    }

    [Test]
    public async Task DeleteTag_PersistsAfterRefresh()
    {
        // Arrange
        var tagName = $"DelPer{Guid.NewGuid():N}"[..10];
        await _editPage.Menu.AddTagAsync(tagName, "#FF0000");
        await WaitForBlazorAsync();

        // Act
        await _editPage.Menu.DeleteTagAsync(tagName);
        await WaitForBlazorAsync();

        // Refresh
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToMenuAsync();
        await _editPage.Menu.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Menu.TagExistsAsync(tagName), Is.False,
            "Deleted tag should not reappear");
    }

    [Test]
    public async Task DeleteAllTags_ListBecomesEmpty()
    {
        // Arrange - add tags to delete
        var tag1 = $"All1{Guid.NewGuid():N}"[..8];
        var tag2 = $"All2{Guid.NewGuid():N}"[..8];
        await _editPage.Menu.AddTagAsync(tag1, "#FF0000");
        await _editPage.Menu.AddTagAsync(tag2, "#00FF00");
        await WaitForBlazorAsync();

        // Act
        await _editPage.Menu.DeleteTagAsync(tag1);
        await _editPage.Menu.DeleteTagAsync(tag2);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Menu.TagExistsAsync(tag1), Is.False);
            Assert.That(await _editPage.Menu.TagExistsAsync(tag2), Is.False);
        });
    }

    #endregion
}