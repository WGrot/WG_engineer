using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using RestaurantApp.Application.Common.Interfaces;
using RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;

namespace RestaurantApp.Infrastructure.Services;

public class S3UrlBuilder: IUrlBuilder
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageConfiguration _config;

    public S3UrlBuilder(IAmazonS3 s3Client, IOptions<StorageConfiguration> config)
    {
        _s3Client = s3Client;
        _config = config.Value;
    }

    public async Task<string> GetPresignedUrlAsync(
        string fileName, 
        string bucketName, 
        int expirationInMinutes = 60)
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

    public string GetPublicUrl(string fileName, string bucketName)
    {
        var protocol = _config.UseSSL ? "https" : "http";

        var endpoint = (_config.PublicEndpoint ?? _config.Endpoint).TrimEnd('/');
        var encodedFileName = Uri.EscapeDataString(fileName);

        return $"{protocol}://{endpoint}/{bucketName}/{encodedFileName}";
    }

    public (string BucketName, string Key) ParseStorageUrl(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            throw new ArgumentException("URL cannot be null or empty", nameof(fileUrl));
        }

        var uri = new Uri(fileUrl);
        
        // Handle MinIO and other S3-compatible storage
        // Format: http://localhost:9000/bucket-name/path/to/file.jpg
        var pathParts = uri.AbsolutePath.TrimStart('/').Split('/', 2);

        if (pathParts.Length < 2)
        {
            throw new ArgumentException(
                $"Invalid URL format. Expected: http://host/bucket/key, got: {fileUrl}");
        }

        var bucketName = pathParts[0];
        var key = Uri.UnescapeDataString(pathParts[1]);

        return (bucketName, key);
    }
}
