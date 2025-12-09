using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Application.Interfaces.Images;
using RestaurantApp.Domain.Enums;
using RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Infrastructure.Services;

public class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IUrlBuilder _urlBuilder;
    private readonly IImageProcessor _imageProcessor;
    private readonly StorageConfiguration _config;
    private readonly ImageSettings _imageSettings;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        IOptions<StorageConfiguration> config,
        IOptions<ImageSettings> imageSettings,
        IUrlBuilder urlBuilder,
        IImageProcessor imageProcessor,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client;
        _config = config.Value;
        _imageSettings = imageSettings.Value;
        _urlBuilder = urlBuilder;
        _imageProcessor = imageProcessor;
        _logger = logger;
    }

    #region Image Operations

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


            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);

            memoryStream.Position = 0;

            var imageInfo = await _imageProcessor.GetImageInfoAsync(memoryStream);


            if (imageInfo == null || !imageInfo.IsValid)
            {
                throw new InvalidOperationException("Invalid image format or corrupted file");
            }

            memoryStream.Position = 0;

            var settings = _imageSettings.GetConfig(imageType);

            using var processingResult = await _imageProcessor.ProcessImageAsync(
                memoryStream,
                settings.MaxWidth,
                settings.MaxHeight,
                settings.Quality);

            var prefix = GetPrefixForImageType(imageType, entityId);
            var uniqueFileName = GenerateUniqueFileName(fileName);
            var fullPath = $"{prefix}{uniqueFileName}";

            var metadata = new Dictionary<string, string>
            {
                { "image-type", imageType.ToString() },
                { "original-filename", fileName },
                { "entity-id", entityId?.ToString() ?? "none" },
                { "width", processingResult.Width.ToString() },
                { "height", processingResult.Height.ToString() }
            };

            processingResult.ProcessedStream.Position = 0;
            await UploadWithMetadataAsync(
                processingResult.ProcessedStream,
                fullPath,
                bucketName,
                "image/jpeg",
                metadata);

            var result = new ImageUploadResult
            {
                FileName = fullPath,
                OriginalUrl = _urlBuilder.GetPublicUrl(fullPath, bucketName),
                FileSize = processingResult.FileSize,
                Metadata = metadata
            };

            if (generateThumbnail)
            {
                memoryStream.Position = 0;

                using var thumbnailSourceStream = new MemoryStream();
                await memoryStream.CopyToAsync(thumbnailSourceStream);
                thumbnailSourceStream.Position = 0;

                var thumbnailPath = await GenerateThumbnailAsync(
                    thumbnailSourceStream,
                    uniqueFileName,
                    settings.ThumbnailSize,
                    settings.Quality);

                result.ThumbnailUrl = _urlBuilder.GetPublicUrl(thumbnailPath, bucketName);
            }

            _logger.LogInformation(
                "Image uploaded successfully: {Path} (Type: {ImageType})",
                fullPath, imageType);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error uploading image {FileName} of type {ImageType}",
                fileName, imageType);
            throw;
        }
    }

    public async Task<bool> DeleteImageWithThumbnailAsync(string fileName, string bucketName)
    {
        try
        {
            await DeleteFileAsync(fileName, bucketName);

            var thumbnailName = $"thumbnails/{Path.GetFileName(fileName)}";
            await DeleteFileAsync(thumbnailName, bucketName);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image with thumbnail: {FileName}", fileName);
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
            if (obj.Key.Contains("/thumbnails/")) continue;

            var metadata = await GetObjectMetadataAsync(obj.Key, bucketName);

            results.Add(new ImageUploadResult
            {
                FileName = obj.Key,
                OriginalUrl = await _urlBuilder.GetPresignedUrlAsync(obj.Key, bucketName),
                FileSize = obj.Size,
                Metadata = metadata
            });
        }

        return results;
    }

    #endregion

    #region Generic File Operations

    public async Task<string> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string bucketName,
        string? contentType = null)
    {
        try
        {
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
                _logger.LogInformation(
                    "File {FileName} uploaded successfully to bucket {BucketName}",
                    uniqueFileName, bucketName);
                return uniqueFileName;
            }

            throw new Exception($"Failed to upload file. Status: {response.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error uploading file {FileName} to bucket {BucketName}",
                fileName, bucketName);
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

            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return memoryStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error downloading file {FileName} from bucket {BucketName}",
                fileName, bucketName);
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

            _logger.LogInformation(
                "File {FileName} deleted from bucket {BucketName}",
                fileName, bucketName);

            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error deleting file {FileName} from bucket {BucketName}",
                fileName, bucketName);
            return false;
        }
    }

    public async Task<bool> DeleteFileByUrlAsync(string fileUrl)
    {
        try
        {
            var (bucketName, key) = _urlBuilder.ParseStorageUrl(fileUrl);

            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request);

            _logger.LogInformation(
                "File {Key} deleted from bucket {BucketName}",
                key, bucketName);

            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("File not found: {FileUrl}", fileUrl);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from URL: {FileUrl}", fileUrl);
            return false;
        }
    }

    public async Task<IEnumerable<StorageFileInfo>> ListFilesAsync(
        string bucketName,
        string? prefix = null)
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

            return response.S3Objects.Select(obj => new StorageFileInfo
            {
                Key = obj.Key,
                Size = obj.Size,
                LastModified = obj.LastModified,
                ETag = obj.ETag
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in bucket {BucketName}", bucketName);
            return Enumerable.Empty<StorageFileInfo>();
        }
    }

    #endregion

    #region Maintenance

    public async Task CleanupOldTempFilesAsync(int daysOld = 7)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var tempBucket = _config.BucketNames.TempFiles;

        var files = await ListFilesAsync(tempBucket);
        var toDelete = files.Where(f => f.LastModified < cutoffDate).ToList();

        if (toDelete.Any())
        {
            var deleteRequest = new DeleteObjectsRequest
            {
                BucketName = tempBucket,
                Objects = toDelete.Select(o => new KeyVersion { Key = o.Key }).ToList()
            };

            await _s3Client.DeleteObjectsAsync(deleteRequest);
            _logger.LogInformation("Cleaned up {Count} old temp files", toDelete.Count);
        }
    }

    #endregion

    #region Private Methods

    private async Task<string> GenerateThumbnailAsync(
        Stream originalStream,
        string fileName,
        int size,
        int quality)
    {
        var bucketName = _config.BucketNames.Images;
        var thumbnailPath = $"thumbnails/{fileName}";

        using var thumbnailStream = await _imageProcessor.CreateThumbnailAsync(
            originalStream,
            size,
            quality);

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

    private async Task<Dictionary<string, string>> GetObjectMetadataAsync(
        string key,
        string bucketName)
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

        if (entityId.HasValue)
        {
            var subFolder = (entityId.Value / 1000) * 1000;
            basePrefix += $"{subFolder}/{entityId}/";
        }

        return basePrefix;
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();

        if (extension != ".jpg" && extension != ".jpeg")
        {
            extension = ".jpg";
        }

        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var shortGuid = Guid.NewGuid().ToString("N")[..8];

        return $"{timestamp}_{shortGuid}{extension}";
    }

    #endregion
}