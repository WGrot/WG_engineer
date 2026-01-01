using RestaurantApp.Domain.Enums;
using RestaurantApp.Domain.Models;
using RestaurantApp.Shared.DTOs.Images;

namespace RestaurantApp.Application.Interfaces.Images;

public interface IStorageService
{
    Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream,
        string fileName,
        ImageType imageType,
        CancellationToken ct,
        int? entityId = null,
        bool generateThumbnail = true);
    Task<bool> DeleteImageWithThumbnailAsync(string fileName, string bucketName);
    Task<IEnumerable<ImageUploadResult>> ListImagesAsync(
        ImageType imageType,
        int? entityId = null,
        int maxResults = 100);
    Task<string> UploadFileAsync(
        Stream fileStream, 
        string fileName, 
        string bucketName, 
        string? contentType = null);
    Task<Stream> DownloadFileAsync(string fileName, string bucketName);
    Task<bool> DeleteFileAsync(string fileName, string bucketName);
    Task<bool> DeleteFileByUrlAsync(string fileUrl);
    
    Task<bool> DeleteByImageLink(ImageLink imageLink);
    Task<IEnumerable<StorageFileInfo>> ListFilesAsync(
        string bucketName, 
        string? prefix = null);
    Task CleanupOldTempFilesAsync(int daysOld = 7);
}