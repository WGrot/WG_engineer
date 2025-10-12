﻿using Amazon.S3.Model;
using RestaurantApp.Api.Common;
using RestaurantApp.Shared.Models;

namespace RestaurantApp.Api.Services.Interfaces;

public interface IStorageService
{
    // Metody dla obrazów
    Task<ImageUploadResult> UploadImageAsync(
        Stream imageStream, 
        string fileName, 
        ImageType imageType, 
        int? entityId = null,
        bool generateThumbnail = true);
    
    Task<bool> DeleteImageWithThumbnailAsync(string fileName, string bucketName);
    
    Task<IEnumerable<ImageUploadResult>> ListImagesAsync(
        ImageType imageType, 
        int? entityId = null,
        int maxResults = 100);
    
    // Metody ogólne dla plików
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string bucketName, string contentType = null);
    Task<Stream> DownloadFileAsync(string fileName, string bucketName);
    Task<bool> DeleteFileAsync(string fileName, string bucketName);
    Task<IEnumerable<S3Object>> ListFilesAsync(string bucketName, string prefix = null);
    Task<string> GetPresignedUrlAsync(string fileName, string bucketName, int expirationInMinutes = 60);
    public string GetImagesBucketName();
    Task<bool> CreateBucketIfNotExistsAsync(string bucketName);
    
}