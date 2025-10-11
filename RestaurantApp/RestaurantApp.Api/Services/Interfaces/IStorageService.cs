using Amazon.S3.Model;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName, string contentType = null);
    Task<Stream> DownloadFileAsync(string fileName, string bucketName);
    Task<bool> DeleteFileAsync(string fileName, string bucketName);
    Task<IEnumerable<S3Object>> ListFilesAsync(string bucketName, string prefix = null);
    Task<string> GetPresignedUrlAsync(string fileName, string bucketName, int expirationInMinutes = 60);
    Task<bool> CreateBucketIfNotExistsAsync(string bucketName);
}