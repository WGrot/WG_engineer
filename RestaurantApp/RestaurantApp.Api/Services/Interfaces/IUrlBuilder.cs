namespace RestaurantApp.Api.Services.Interfaces;

public interface IUrlBuilder
{
    Task<string> GetPresignedUrlAsync(string fileName, string bucketName, int expirationInMinutes = 60);
    string GetPublicUrl(string fileName, string bucketName);
    public (string bucketName, string key) ParseS3OrMinioUrl(string fileUrl);

}