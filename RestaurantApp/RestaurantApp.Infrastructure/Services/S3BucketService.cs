using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RestaurantApp.Infrastructure.Persistence.Configurations.Configuration;
using System.Text.Json;
using RestaurantApp.Application.Interfaces.Images;

namespace RestaurantApp.Infrastructure.Services;

public class S3BucketService: IBucketService
{
    private readonly IAmazonS3 _s3Client;
    private readonly StorageConfiguration _config;
    private readonly ILogger<S3BucketService> _logger;

    public S3BucketService(
        IAmazonS3 s3Client, 
        IOptions<StorageConfiguration> config,
        ILogger<S3BucketService> logger)
    {
        _s3Client = s3Client;
        _config = config.Value;
        _logger = logger;
    }

    public async Task<bool> EnsureBucketExistsAsync(string bucketName)
    {
        try
        {
            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
            
            if (!exists)
            {
                var request = new PutBucketRequest
                {
                    BucketName = bucketName,
                    UseClientRegion = true
                };

                await _s3Client.PutBucketAsync(request);
                _logger.LogInformation("Created bucket: {BucketName}", bucketName);
                
                if (bucketName == _config.BucketNames.Images)
                {
                    await SetBucketPolicyAsync(bucketName, allowPublicRead: true);
                }
                else
                {
                    await SetBucketPolicyAsync(bucketName, allowPublicRead: false);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring bucket exists: {BucketName}", bucketName);
            return false;
        }
    }

    public async Task SetBucketPolicyAsync(string bucketName, bool allowPublicRead = true)
    {
        try
        {
            string policy;
            
            if (allowPublicRead)
            {
                policy = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Allow",
                            "Principal": {"AWS": ["*"]},
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
                        }
                    ]
                }
                """;
            }
            else
            {
                policy = $$"""
                {
                    "Version": "2012-10-17",
                    "Statement": [
                        {
                            "Effect": "Deny",
                            "Principal": {"AWS": ["*"]},
                            "Action": ["s3:GetObject"],
                            "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
                        }
                    ]
                }
                """;
            }

            var request = new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policy
            };

            await _s3Client.PutBucketPolicyAsync(request);
            
            _logger.LogInformation(
                "Set bucket policy for {BucketName} (PublicRead: {AllowPublicRead})", 
                bucketName, 
                allowPublicRead);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex, 
                "Error setting bucket policy for {BucketName}", 
                bucketName);
            throw;
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

        _logger.LogInformation("Default buckets initialized with policies");
    }

    public async Task<string> GetBucketPolicyAsync(string bucketName)
    {
        try
        {
            var request = new GetBucketPolicyRequest
            {
                BucketName = bucketName
            };

            var response = await _s3Client.GetBucketPolicyAsync(request);
            return response.Policy;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucketPolicy")
        {
            _logger.LogWarning("No policy found for bucket: {BucketName}", bucketName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting bucket policy: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task DeleteBucketAsync(string bucketName)
    {
        try
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await _s3Client.ListObjectsV2Async(listRequest);

                if (listResponse.S3Objects.Any())
                {
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = bucketName,
                        Objects = listResponse.S3Objects
                            .Select(o => new KeyVersion { Key = o.Key })
                            .ToList()
                    };

                    await _s3Client.DeleteObjectsAsync(deleteRequest);
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;

            } while (listResponse.IsTruncated == true);
            
            await _s3Client.DeleteBucketAsync(bucketName);
            _logger.LogInformation("Deleted bucket: {BucketName}", bucketName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting bucket: {BucketName}", bucketName);
            throw;
        }
    }
}