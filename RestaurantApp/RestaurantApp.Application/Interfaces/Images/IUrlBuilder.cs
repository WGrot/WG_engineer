namespace RestaurantApp.Application.Common.Interfaces;

public interface IUrlBuilder
{
    Task<string> GetPresignedUrlAsync(
        string fileName, 
        string bucketName, 
        int expirationInMinutes = 60);
    
    string GetPublicUrl(string fileName, string bucketName);
    
    (string BucketName, string Key) ParseStorageUrl(string fileUrl);
}