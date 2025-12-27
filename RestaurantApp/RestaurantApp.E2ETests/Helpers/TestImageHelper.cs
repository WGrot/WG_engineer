using SkiaSharp;

namespace RestaurantApp.E2ETests.Helpers;

public static class TestImageHelper
{
    private static readonly string TestFilesDirectory = Path.Combine(
        Path.GetTempPath(), 
        "RestaurantAppE2ETests");

    static TestImageHelper()
    {
        Directory.CreateDirectory(TestFilesDirectory);
    }

    public static string CreateTestImage(
        string fileName, 
        int width = 200, 
        int height = 200, 
        SKColor? color = null)
    {
        var filePath = Path.Combine(TestFilesDirectory, fileName);
        color ??= SKColors.CornflowerBlue;

        using var bitmap = new SKBitmap(width, height);
        using var canvas = new SKCanvas(bitmap);
        
        canvas.Clear(color.Value);
        
        using var paint = new SKPaint
        {
            Color = SKColors.White,
            StrokeWidth = 2,
            IsAntialias = true
        };
        
        canvas.DrawLine(0, 0, width, height, paint);
        canvas.DrawLine(width, 0, 0, height, paint);


        using var textPaint = new SKPaint
        {
            Color = SKColors.White,
            TextSize = 14,
            IsAntialias = true,
            TextAlign = SKTextAlign.Center
        };
        
        canvas.DrawText(Path.GetFileNameWithoutExtension(fileName), 
            width / 2f, height / 2f, textPaint);

        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.OpenWrite(filePath);
        data.SaveTo(stream);

        return filePath;
    }


    public static string[] CreateTestImages(int count, string prefix = "test_image")
    {
        var colors = new[]
        {
            SKColors.CornflowerBlue,
            SKColors.Coral,
            SKColors.MediumSeaGreen,
            SKColors.MediumPurple,
            SKColors.Gold,
            SKColors.Tomato,
            SKColors.Teal,
            SKColors.SlateBlue,
            SKColors.OrangeRed,
            SKColors.DarkCyan
        };

        var paths = new string[count];
        for (int i = 0; i < count; i++)
        {
            var color = colors[i % colors.Length];
            paths[i] = CreateTestImage($"{prefix}_{i + 1}.png", color: color);
        }

        return paths;
    }

    public static string CreateProfilePhoto(string fileName = "profile_test.png")
    {
        return CreateTestImage(fileName, 400, 400, SKColors.DarkSlateBlue);
    }


    public static string[] CreateGalleryPhotos(int count = 3)
    {
        return CreateTestImages(count, "gallery");
    }


    public static void CleanupTestImages()
    {
        if (Directory.Exists(TestFilesDirectory))
        {
            try
            {
                Directory.Delete(TestFilesDirectory, recursive: true);
            }
            catch
            {
            }
        }
    }
    
    public static string GetTestFilesDirectory() => TestFilesDirectory;
}

