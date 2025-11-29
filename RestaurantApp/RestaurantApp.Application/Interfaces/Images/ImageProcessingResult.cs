namespace RestaurantApp.Application.Interfaces.Images;

public record ImageProcessingResult(
    Stream ProcessedStream,
    int Width,
    int Height,
    long FileSize
) : IDisposable
{
    public void Dispose()
    {
        ProcessedStream.Dispose();
    }
}

public record ImageInfo(
    int Width,
    int Height,
    bool IsValid
);