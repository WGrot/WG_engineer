namespace RestaurantApp.Api.Services.Interfaces;

public interface IBucketService
{
    Task<bool> EnsureBucketExistsAsync(string bucketName);
    Task InitializeDefaultBucketsAsync();
    Task DeleteBucketAsync(string bucketName);
}