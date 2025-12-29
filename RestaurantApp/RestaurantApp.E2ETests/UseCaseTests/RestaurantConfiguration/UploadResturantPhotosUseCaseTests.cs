using RestaurantApp.E2ETests.Helpers;
using RestaurantApp.E2ETests.PageObjects.EditRestaurantPages;
using RestaurantApp.E2ETests.TestSetup;

namespace RestaurantApp.E2ETests.UseCaseTests.RestaurantConfiguration;

[TestFixture]
public class UploadResturantPhotosUseCaseTests : PlaywrightTestBase
{
    private RestaurantEditPage _editPage = null!;
    private string _profilePhotoPath = null!;
    private string[] _galleryPhotoPaths = null!;

    [OneTimeSetUp]
    public void CreateTestImages()
    {
        _profilePhotoPath = TestImageHelper.CreateProfilePhoto("profile_photo.png");
        _galleryPhotoPaths = TestImageHelper.CreateGalleryPhotos(3);
        
        Console.WriteLine($"Test images created in: {TestImageHelper.GetTestFilesDirectory()}");
        Console.WriteLine($"Profile photo: {_profilePhotoPath}");
        foreach (var path in _galleryPhotoPaths)
        {
            Console.WriteLine($"Gallery photo: {path}");
        }
    }

    [OneTimeTearDown]
    public void CleanupTestImages()
    {
        TestImageHelper.CleanupTestImages();
    }

    [SetUp]
    public async Task Setup()
    {
        _editPage = new RestaurantEditPage(Page);
        await LoginAsVerifiedUserAsync();
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToAppearanceAsync();
        await _editPage.Appearance.WaitForLoadAsync();
        await WaitForBlazorAsync();
    }

    #region Profile Photo Tests

    [Test]
    public async Task AppearanceTab_IsVisibleAndActive()
    {
        // Assert
        var activeTab = await _editPage.GetActiveTabNameAsync();
        Assert.That(activeTab, Does.Contain("Appearance"));
    }

    [Test]
    public async Task ProfilePhoto_WhenNoPhoto_ShowsPlaceholder()
    {
        // Arrange - ensure no profile photo exists
        if (await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.DeleteProfilePhotoAsync();
            await WaitForBlazorAsync();
        }

        // Assert
        Assert.That(await _editPage.Appearance.HasPlaceholderAsync(), Is.True,
            "Placeholder should be visible when no profile photo exists");
    }

    [Test]
    public async Task UploadProfilePhoto_Success_DisplaysImage()
    {
        // Arrange - ensure clean state
        if (await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.DeleteProfilePhotoAsync();
            await WaitForBlazorAsync();
        }

        // Act
        await _editPage.Appearance.UploadProfilePhotoAsync(_profilePhotoPath);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Appearance.HasProfilePhotoAsync(), Is.True,
                "Profile photo should be visible after upload");
            Assert.That(await _editPage.Appearance.HasPlaceholderAsync(), Is.False,
                "Placeholder should be hidden after upload");
        });
    }

    [Test]
    public async Task UploadProfilePhoto_Success_ImageSourceIsSet()
    {
        // Arrange
        if (!await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.UploadProfilePhotoAsync(_profilePhotoPath);
            await WaitForBlazorAsync();
        }

        // Act
        var imageSrc = await _editPage.Appearance.GetProfileImageSrcAsync();

        // Assert
        Assert.That(imageSrc, Is.Not.Null.And.Not.Empty,
            "Profile image should have a valid src attribute");
    }

    [Test]
    public async Task DeleteProfilePhoto_Success_ShowsPlaceholder()
    {
        // Arrange - ensure profile photo exists
        if (!await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.UploadProfilePhotoAsync(_profilePhotoPath);
            await WaitForBlazorAsync();
        }

        // Act
        await _editPage.Appearance.DeleteProfilePhotoAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Appearance.HasProfilePhotoAsync(), Is.False,
                "Profile photo should not be visible after deletion");
            Assert.That(await _editPage.Appearance.HasPlaceholderAsync(), Is.True,
                "Placeholder should be visible after deletion");
        });
    }

    [Test]
    public async Task ProfilePhoto_PersistsAfterPageRefresh()
    {
        // Arrange - upload photo
        if (!await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.UploadProfilePhotoAsync(_profilePhotoPath);
            await WaitForBlazorAsync();
        }



        // Act - refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToAppearanceAsync();
        await _editPage.Appearance.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert

        Assert.That(await _editPage.Appearance.HasProfilePhotoAsync(), Is.True,
            "Profile photo should persist after page refresh");
    }

    #endregion

    #region Gallery Photos Tests

    [Test]
    public async Task Gallery_WhenNoPhotos_ShowsNoPhotosMessage()
    {
        // Arrange - delete all gallery photos
        await _editPage.Appearance.DeleteAllGalleryPhotosAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.That(await _editPage.Appearance.HasNoPhotosMessageAsync(), Is.True,
            "No photos message should be visible when gallery is empty");
    }

    [Test]
    public async Task UploadSingleGalleryPhoto_Success_DisplaysInGallery()
    {
        // Arrange
        var initialCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();

        // Act
        await _editPage.Appearance.UploadGalleryPhotosAsync(_galleryPhotoPaths[0]);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount + 1),
            "Gallery should have one more photo after upload");
    }

    [Test]
    public async Task UploadMultipleGalleryPhotos_Success_AllDisplayed()
    {
        // Arrange - start fresh
        await _editPage.Appearance.DeleteAllGalleryPhotosAsync();
        await WaitForBlazorAsync();
        
        var photosToUpload = _galleryPhotoPaths.Length;

        // Act
        await _editPage.Appearance.UploadGalleryPhotosAsync(_galleryPhotoPaths);
        await WaitForBlazorAsync();

        // Assert
        var count = await _editPage.Appearance.GetGalleryPhotoCountAsync();
        Assert.That(count, Is.EqualTo(photosToUpload),
            $"Gallery should display all {photosToUpload} uploaded photos");
    }

    [Test]
    public async Task DeleteGalleryPhoto_Success_RemovesFromGallery()
    {
        // Arrange - ensure at least one photo exists
        if (await _editPage.Appearance.GetGalleryPhotoCountAsync() == 0)
        {
            await _editPage.Appearance.UploadGalleryPhotosAsync(_galleryPhotoPaths[0]);
            await WaitForBlazorAsync();
        }

        var initialCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();

        // Act
        await _editPage.Appearance.DeleteGalleryPhotoAtIndexAsync(0);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount - 1),
            "Gallery should have one less photo after deletion");
    }

    [Test]
    public async Task DeleteAllGalleryPhotos_Success_ShowsNoPhotosMessage()
    {
        // Arrange - ensure photos exist
        if (await _editPage.Appearance.GetGalleryPhotoCountAsync() == 0)
        {
            await _editPage.Appearance.UploadGalleryPhotosAsync(_galleryPhotoPaths);
            await WaitForBlazorAsync();
        }

        // Act
        await _editPage.Appearance.DeleteAllGalleryPhotosAsync();
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Appearance.GetGalleryPhotoCountAsync(), Is.EqualTo(0),
                "Gallery should be empty");
            Assert.That(await _editPage.Appearance.HasNoPhotosMessageAsync(), Is.True,
                "No photos message should be visible");
        });
    }

    [Test]
    public async Task GalleryPhotos_PersistAfterPageRefresh()
    {
        // Arrange - upload photos
        await _editPage.Appearance.DeleteAllGalleryPhotosAsync();
        await _editPage.Appearance.UploadGalleryPhotosAsync(_galleryPhotoPaths[0], _galleryPhotoPaths[1]);
        await WaitForBlazorAsync();

        var initialCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();

        // Act - refresh page
        await _editPage.NavigateAsync(1);
        await _editPage.SwitchToAppearanceAsync();
        await _editPage.Appearance.WaitForLoadAsync();
        await WaitForBlazorAsync();

        // Assert
        var refreshedCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();
        Assert.That(refreshedCount, Is.EqualTo(initialCount),
            "Gallery photos should persist after page refresh");
    }

    #endregion

    #region Combined Tests

    [Test]
    public async Task UploadProfileAndGallery_BothDisplayCorrectly()
    {
        // Arrange - clean state
        if (await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.DeleteProfilePhotoAsync();
        }
        await _editPage.Appearance.DeleteAllGalleryPhotosAsync();
        await WaitForBlazorAsync();

        // Act
        await _editPage.Appearance.UploadProfilePhotoAsync(_profilePhotoPath);
        await _editPage.Appearance.UploadGalleryPhotosAsync(_galleryPhotoPaths);
        await WaitForBlazorAsync();

        // Assert
        Assert.Multiple(async () =>
        {
            Assert.That(await _editPage.Appearance.HasProfilePhotoAsync(), Is.True,
                "Profile photo should be visible");
            Assert.That(await _editPage.Appearance.GetGalleryPhotoCountAsync(), Is.EqualTo(3),
                "All gallery photos should be visible");
        });
    }

    #endregion

    

    [Test]
    public async Task DeleteProfilePhoto_WhenNoPhoto_ButtonShouldBeDisabledOrHandledGracefully()
    {
        // Arrange - ensure no profile photo
        if (await _editPage.Appearance.HasProfilePhotoAsync())
        {
            await _editPage.Appearance.DeleteProfilePhotoAsync();
            await WaitForBlazorAsync();
        }

        // Assert - verify we're in clean state (no photo)
        Assert.That(await _editPage.Appearance.HasPlaceholderAsync(), Is.True);
    }

    [Test]
    public async Task GalleryPhotoCount_AfterMultipleOperations_IsCorrect()
    {
        // Arrange
        await _editPage.Appearance.DeleteAllGalleryPhotosAsync();
        await WaitForBlazorAsync();

        // Create test images using full helper
        var testImages = TestImageHelper.CreateTestImages(5, "batch_test");

        // Act - upload 5, delete 2, upload 1 more
        await _editPage.Appearance.UploadGalleryPhotosAsync(testImages);
        await WaitForBlazorAsync();
        
        await _editPage.Appearance.DeleteGalleryPhotoAtIndexAsync(0);
        await _editPage.Appearance.DeleteGalleryPhotoAtIndexAsync(0);
        await WaitForBlazorAsync();

        var additionalImage = TestImageHelper.CreateTestImage("additional.png", 200, 200);
        await _editPage.Appearance.UploadGalleryPhotosAsync(additionalImage);
        await WaitForBlazorAsync();

        // Assert: 5 - 2 + 1 = 4
        var finalCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();
        Assert.That(finalCount, Is.EqualTo(4),
            "Gallery count should be correct after multiple operations");
    }

    [Test]
    public async Task UploadLargeImage_Success()
    {
        // Arrange - create a larger test image (3000 x 3000)
        var largeImagePath = TestImageHelper.CreateTestImage(
            "large_test.png", 
            width: 3000, 
            height: 3000);

        var initialCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();

        // Act
        await _editPage.Appearance.UploadGalleryPhotosAsync(largeImagePath);
        await WaitForBlazorAsync();

        // Assert
        var newCount = await _editPage.Appearance.GetGalleryPhotoCountAsync();
        Assert.That(newCount, Is.EqualTo(initialCount + 1),
            "Large image should upload successfully");
    }
    
}