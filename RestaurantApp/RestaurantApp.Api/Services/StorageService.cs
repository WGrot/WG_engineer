using System.Net.Mime;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RestaurantApp.Api.Common;
using RestaurantApp.Api.Configuration;
using RestaurantApp.Api.Services.Interfaces;
using RestaurantApp.Shared.Models;
using SkiaSharp;

namespace RestaurantApp.Api.Services;

public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageConfiguration _config;
    private readonly ILogger<StorageService> _logger;
    
    // Konfiguracja rozmiarów dla różnych typów zdjęć
    private readonly Dictionary<ImageType, (int maxWidth, int maxHeight, int thumbnailSize)> _imageSizes = new()
    {
        { ImageType.UserProfile, (800, 800, 150) },
        { ImageType.RestaurantProfile, (1200, 1200, 200) },
        { ImageType.RestaurantBackground, (1920, 1080, 300) },
        { ImageType.MenuItem, (1000, 1000, 250) },
        { ImageType.RestaurantPhotos, (1200, 1200, 200) }
    };

    public StorageService(IAmazonS3 s3Client, IOptions<StorageConfiguration> config,
        ILogger<StorageService> logger)
    {
        _s3Client = s3Client;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream, 
        string fileName, 
        ImageType imageType, 
        int? entityId = null,
        bool generateThumbnail = true)
    {
        try
        {
            var bucketName = _config.BucketNames.Images;
            await CreateBucketIfNotExistsAsync(bucketName);
            
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            // Generuj ścieżkę na podstawie typu obrazu
            var prefix = GetPrefixForImageType(imageType, entityId);
            
            // Walidacja i przetwarzanie obrazu
            using var originalBitmap = SKBitmap.Decode(memoryStream);
            if (originalBitmap == null)
            {
                throw new InvalidOperationException("Invalid image format or corrupted file");
            }
            
            // Pobierz konfigurację rozmiarów
            var (maxWidth, maxHeight, thumbnailSize) = _imageSizes[imageType];
            
            // Resize jeśli za duży
            SKBitmap processedBitmap = originalBitmap;
            var needsResize = originalBitmap.Width > maxWidth || originalBitmap.Height > maxHeight;
            
            if (needsResize)
            {
                processedBitmap = ResizeImage(originalBitmap, maxWidth, maxHeight);
            }

            // Zapisz główny obraz
            var uniqueFileName = GenerateUniqueFileName(fileName);
            var fullPath = $"{prefix}{uniqueFileName}";
            
            using var processedStream = new MemoryStream();
            SaveImageAsJpeg(processedBitmap, processedStream, 85);
            processedStream.Position = 0;

            var metadata = new Dictionary<string, string>
            {
                { "image-type", imageType.ToString() },
                { "original-filename", fileName },
                { "entity-id", entityId?.ToString() ?? "none" },
                { "width", processedBitmap.Width.ToString() },
                { "height", processedBitmap.Height.ToString() }
            };
            var fileSize = processedStream.Length;
            processedStream.Position = 0;
            await UploadWithMetadataAsync(processedStream, fullPath, bucketName, "image/jpeg", metadata);

            var result = new ImageUploadResult
            {
                FileName = fullPath,
                OriginalUrl = await GetPresignedUrlAsync(fullPath, bucketName),
                FileSize = fileSize,
                Metadata = metadata
            };

            // Generuj miniaturkę jeśli wymagana
            if (generateThumbnail)
            {
                var thumbnailPath = await GenerateThumbnailAsync(originalBitmap, uniqueFileName, thumbnailSize);
                result.ThumbnailUrl = await GetPresignedUrlAsync(thumbnailPath, bucketName);
            }

            // Cleanup
            if (needsResize && processedBitmap != originalBitmap)
            {
                processedBitmap.Dispose();
            }

            _logger.LogInformation($"Image uploaded successfully: {fullPath} (Type: {imageType})");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading image {fileName} of type {imageType}");
            throw;
        }
    }
    
    private SKBitmap ResizeImage(SKBitmap original, int maxWidth, int maxHeight)
    {
        // Oblicz proporcje
        var ratioX = (float)maxWidth / original.Width;
        var ratioY = (float)maxHeight / original.Height;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(original.Width * ratio);
        var newHeight = (int)(original.Height * ratio);

        // Resize z wysoką jakością
        var resized = original.Resize(new SKImageInfo(newWidth, newHeight), SKFilterQuality.High);
        
        if (resized == null)
        {
            throw new InvalidOperationException("Failed to resize image");
        }

        return resized;
    }
    
    private SKBitmap CreateSquareThumbnail(SKBitmap original, int size)
    {
        // Oblicz kwadratowy crop
        int sourceSize = Math.Min(original.Width, original.Height);
        int xOffset = (original.Width - sourceSize) / 2;
        int yOffset = (original.Height - sourceSize) / 2;

        // Najpierw cropuj do kwadratu
        var cropped = new SKBitmap(sourceSize, sourceSize);
        using (var canvas = new SKCanvas(cropped))
        {
            var sourceRect = SKRect.Create(xOffset, yOffset, sourceSize, sourceSize);
            var destRect = SKRect.Create(0, 0, sourceSize, sourceSize);
            canvas.DrawBitmap(original, sourceRect, destRect);
        }

        // Następnie zmniejsz do docelowego rozmiaru
        var thumbnail = cropped.Resize(new SKImageInfo(size, size), SKFilterQuality.High);
        cropped.Dispose();

        if (thumbnail == null)
        {
            throw new InvalidOperationException("Failed to create thumbnail");
        }

        return thumbnail;
    }

    private void SaveImageAsJpeg(SKBitmap bitmap, Stream stream, int quality)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);
        data.SaveTo(stream);
    }

    private async Task<string> GenerateThumbnailAsync(SKBitmap originalBitmap, string fileName, int size)
    {
        var bucketName = _config.BucketNames.Images;
        var thumbnailPath = $"thumbnails/{fileName}";

        // Utwórz kwadratową miniaturkę
        using var thumbnail = CreateSquareThumbnail(originalBitmap, size);
        
        using var thumbnailStream = new MemoryStream();
        SaveImageAsJpeg(thumbnail, thumbnailStream, 75);
        thumbnailStream.Position = 0;

        await UploadWithMetadataAsync(
            thumbnailStream, 
            thumbnailPath, 
            bucketName, 
            "image/jpeg",
            new Dictionary<string, string> { { "is-thumbnail", "true" } });

        return thumbnailPath;
    }

    private async Task UploadWithMetadataAsync(
        Stream stream, 
        string key, 
        string bucketName, 
        string contentType,
        Dictionary<string, string> metadata)
    {
        var request = new PutObjectRequest
        {
            BucketName = bucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType,
            ServerSideEncryptionMethod = ServerSideEncryptionMethod.None
        };

        // Dodaj metadane
        foreach (var item in metadata)
        {
            request.Metadata.Add(item.Key, item.Value);
        }

        var response = await _s3Client.PutObjectAsync(request);
        
        if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
        {
            throw new Exception($"Failed to upload file. Status: {response.HttpStatusCode}");
        }
    }

    private string GetPrefixForImageType(ImageType imageType, int? entityId)
    {
        var basePrefix = imageType switch
        {
            ImageType.UserProfile => "users/profiles/",
            ImageType.RestaurantPhotos => "restaurants/photos/",
            ImageType.RestaurantBackground => "restaurants/backgrounds/",
            ImageType.MenuItem => "restaurants/menu-items/",
            ImageType.RestaurantProfile => "restaurants/profiles/",
            _ => "misc/"
        };

        // Dodaj podkatalog na podstawie ID dla lepszej organizacji
        if (entityId.HasValue)
        {
            // Grupuj po ID (np. co 1000 plików w folderze)
            var subFolder = (entityId.Value / 1000) * 1000;
            basePrefix += $"{subFolder}/{entityId}/";
        }

        return basePrefix;
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        // Konwertuj wszystko na .jpg dla konsystencji
        if (extension != ".jpg" && extension != ".jpeg")
        {
            extension = ".jpg";
        }
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
        return $"{timestamp}_{shortGuid}{extension}";
    }

    public async Task<bool> DeleteImageWithThumbnailAsync(string fileName, string bucketName)
    {
        try
        {
            // Usuń główny obraz
            await DeleteFileAsync(fileName, bucketName);
            
            // Usuń miniaturkę jeśli istnieje
            var thumbnailName = $"thumbnails/{Path.GetFileName(fileName)}";
            await DeleteFileAsync(thumbnailName, bucketName);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting image with thumbnail: {fileName}");
            return false;
        }
    }

    public async Task<IEnumerable<ImageUploadResult>> ListImagesAsync(
        ImageType imageType, 
        int? entityId = null,
        int maxResults = 100)
    {
        var bucketName = _config.BucketNames.Images;
        var prefix = GetPrefixForImageType(imageType, entityId);
        
        var request = new ListObjectsV2Request
        {
            BucketName = bucketName,
            Prefix = prefix,
            MaxKeys = maxResults
        };

        var response = await _s3Client.ListObjectsV2Async(request);
        var results = new List<ImageUploadResult>();

        foreach (var obj in response.S3Objects)
        {
            // Pomiń miniaturki w głównej liście
            if (obj.Key.Contains("/thumbnails/")) continue;

            var metadata = await GetObjectMetadataAsync(obj.Key, bucketName);
            
            results.Add(new ImageUploadResult
            {
                FileName = obj.Key,
                OriginalUrl = await GetPresignedUrlAsync(obj.Key, bucketName),
                FileSize = obj.Size,
                Metadata = metadata
            });
        }

        return results;
    }

    private async Task<Dictionary<string, string>> GetObjectMetadataAsync(string key, string bucketName)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectMetadataAsync(request);
            return response.Metadata.Keys.ToDictionary(k => k, k => response.Metadata[k]);
        }
        catch
        {
            return new Dictionary<string, string>();
        }
    }
    public string GetImagesBucketName()
    {
        return _config.BucketNames.Images;
    }

    // Metoda do czyszczenia starych plików tymczasowych
    public async Task CleanupOldTempFilesAsync(int daysOld = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var tempBucket = _config.BucketNames.TempFiles;
        
        var objects = await ListFilesAsync(tempBucket);
        var toDelete = objects.Where(o => o.LastModified < cutoffDate).ToList();
        
        if (toDelete.Any())
        {
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = tempBucket,
                Objects = toDelete.Select(o => new KeyVersion { Key = o.Key }).ToList()
            };
            
            await _s3Client.DeleteObjectsAsync(deleteRequest);
            _logger.LogInformation($"Cleaned up {toDelete.Count} old temp files");
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName,
        string contentType = null)
    {
        try
        {
            // Ensure bucket exists
            await CreateBucketIfNotExistsAsync(bucketName);

            // Generate unique file name to avoid conflicts
            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = uniqueFileName,
                InputStream = fileStream,
                ContentType = contentType ?? "application/octet-stream",
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.None
            };

            var response = await _s3Client.PutObjectAsync(request);

            if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
            {
                _logger.LogInformation($"File {uniqueFileName} uploaded successfully to bucket {bucketName}");
                return uniqueFileName;
            }

            throw new Exception($"Failed to upload file. Status: {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error uploading file {fileName} to bucket {bucketName}");
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string fileName, string bucketName)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            var response = await _s3Client.GetObjectAsync(request);

            // Copy to memory stream to avoid disposal issues
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file {fileName} from bucket {bucketName}");
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName, string bucketName)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = fileName
            };

            var response = await _s3Client.DeleteObjectAsync(request);

            _logger.LogInformation($"File {fileName} deleted from bucket {bucketName}");
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file {fileName} from bucket {bucketName}");
            return false;
        }
    }

    public async Task<IEnumerable<S3Object>> ListFilesAsync(string bucketName, string prefix = null)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix,
                MaxKeys = 1000
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            return response.S3Objects;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error listing files in bucket {bucketName}");
            return new List<S3Object>();
        }
    }

    public async Task<string> GetPresignedUrlAsync(string fileName, string bucketName, int expirationInMinutes = 60)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = fileName,
                Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes),
                Protocol = _config.UseSSL ? Protocol.HTTPS : Protocol.HTTP
            };

            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating presigned URL for {fileName}");
            throw;
        }
    }

    public async Task<bool> CreateBucketIfNotExistsAsync(string bucketName)
    {
        try
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists)
            {
                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = bucketName
                });

                _logger.LogInformation($"Created bucket: {bucketName}");
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating bucket {bucketName}");
            return false;
        }
    }
}