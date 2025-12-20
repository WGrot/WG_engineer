namespace RestaurantApp.Application.Interfaces.Images;

public interface IBucketService
{
    Task<bool> EnsureBucketExistsAsync(string bucketName);
    Task InitializeDefaultBucketsAsync();
    Task DeleteBucketAsync(string bucketName);
    Task SetBucketPolicyAsync(string bucketName, bool allowPublicRead = true);
    Task<string> GetBucketPolicyAsync(string bucketName);
}