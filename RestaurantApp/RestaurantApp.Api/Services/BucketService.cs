using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Options;
using RestaurantApp.Api.Configuration;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Services;

public class BucketService : IBucketService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageConfiguration _config;

    public BucketService(
        IAmazonS3 s3Client,
        IOptions<StorageConfiguration> config)
    {
        _s3Client = s3Client;
        _config = config.Value;
    }

    public async Task<bool> EnsureBucketExistsAsync(string bucketName)
    {
        try
        {
            var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            if (!bucketExists)
            {
                // Tworzenie bucketa
                await _s3Client.PutBucketAsync(new PutBucketRequest
                {
                    BucketName = bucketName
                });
            }

            // Ustaw publiczną politykę dostępu (dla nowych i istniejących bucketów)
            await SetBucketPublicReadPolicyAsync(bucketName);

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    public async Task InitializeDefaultBucketsAsync()
    {
        var buckets = new[]
        {
            _config.BucketNames.Images,
            _config.BucketNames.Documents,
            _config.BucketNames.TempFiles
        };

        foreach (var bucket in buckets)
        {
            await EnsureBucketExistsAsync(bucket);
        }
    }

    public async Task DeleteBucketAsync(string bucketName)
    {
        await _s3Client.DeleteBucketAsync(new DeleteBucketRequest
        {
            BucketName = bucketName
        });
    }

    private async Task SetBucketPublicReadPolicyAsync(string bucketName)
    {
        // Polityka pozwalająca na publiczny odczyt
        var bucketPolicy = $@"{{
            ""Version"": ""2012-10-17"",
            ""Statement"": [
                {{
                    ""Sid"": ""PublicReadGetObject"",
                    ""Effect"": ""Allow"",
                    ""Principal"": {{
                        ""AWS"": ""*""
                    }},
                    ""Action"": ""s3:GetObject"",
                    ""Resource"": ""arn:aws:s3:::{bucketName}/*""
                }}
            ]
        }}";

        await _s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
        {
            BucketName = bucketName,
            Policy = bucketPolicy
        });
    }
}