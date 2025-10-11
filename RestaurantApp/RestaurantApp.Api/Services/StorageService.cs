using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RestaurantApp.Api.Configuration;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Services;

public class StorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageConfiguration _config;
    private readonly ILogger<StorageService> _logger;

    public StorageService(IAmazonS3 s3Client, IOptions<StorageConfiguration> config,
        ILogger<StorageService> logger)
    {
        _s3Client = s3Client;
        _config = config.Value;
        _logger = logger;
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