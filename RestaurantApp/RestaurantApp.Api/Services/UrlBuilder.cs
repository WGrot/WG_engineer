using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using RestaurantApp.Api.Configuration;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Services;

public class UrlBuilder : IUrlBuilder
{
    private readonly StorageConfiguration _config;
    private readonly IAmazonS3 _s3Client;

    public UrlBuilder(IAmazonS3 s3Client, IOptions<StorageConfiguration> config)
    {
        _s3Client = s3Client;
        _config = config.Value;
    }

    public async Task<string> GetPresignedUrlAsync(string fileName, string bucketName, int expirationInMinutes = 60)
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
        // Budowanie publicznego URL-a
        var protocol = _config.UseSSL ? "https" : "http";
        var endpoint = _config.Endpoint;

        // Usuń trailing slash jeśli istnieje
        endpoint = endpoint.TrimEnd('/');

        // Zakoduj nazwę pliku dla URL
        var encodedFileName = Uri.EscapeDataString(fileName);

        // Format: http://localhost:9000/bucket-name/path/to/file.jpg
        var publicUrl = $"{protocol}://{endpoint}/{bucketName}/{encodedFileName}";
        
        return publicUrl;
    }

    public (string bucketName, string key) ParseS3OrMinioUrl(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
            throw new ArgumentException("URL cannot be null or empty", nameof(fileUrl));

        var uri = new Uri(fileUrl);
    
        // Sprawdzenie czy to URL AWS S3
        if (uri.Host.Contains("amazonaws.com"))
        {
            var s3Uri = new AmazonS3Uri(fileUrl);
            return (s3Uri.Bucket, s3Uri.Key);
        }
    
        // Obsługa MinIO i innych S3-compatible storage
        // Format: http://localhost:9000/bucket-name/path/to/file.jpg
        var pathParts = uri.AbsolutePath.TrimStart('/').Split('/', 2);
    
        if (pathParts.Length < 2)
            throw new ArgumentException($"Invalid URL format. Expected: http://host/bucket/key, got: {fileUrl}");
    
        var bucketName = pathParts[0];
        var key = Uri.UnescapeDataString(pathParts[1]); // Dekodowanie %2F na / itp.
    
        return (bucketName, key);
    }
}