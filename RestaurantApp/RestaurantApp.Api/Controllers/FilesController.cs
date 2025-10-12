using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RestaurantApp.Api.Configuration;
using RestaurantApp.Api.Services.Interfaces;

namespace RestaurantApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FilesController : ControllerBase
{
    private readonly IStorageService _storageService;
    private readonly StorageConfiguration _config;
    private readonly ILogger<FilesController> _logger;

    public FilesController(
        IStorageService storageService,
        IOptions<StorageConfiguration> config,
        ILogger<FilesController> logger)
    {
        _storageService = storageService;
        _config = config.Value;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadFile(
        IFormFile file,
        [FromQuery] string category = "general")
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(new { error = "File is required" });
        }

        // Validate file size (e.g., max 10MB)
        const long maxSize = 10 * 1024 * 1024; // 10MB
        if (file.Length > maxSize)
        {
            return BadRequest(new { error = "File size exceeds 10MB limit" });
        }

        try
        {
            // Determine bucket based on category or file type
            string bucketName = DetermineBucket(file, category);

            using var stream = file.OpenReadStream();
            var fileName = await _storageService.UploadFileAsync(
                stream,
                file.FileName,
                bucketName,
                file.ContentType);

            // Generate presigned URL for immediate access
            var url = await _storageService.GetPresignedUrlAsync(fileName, bucketName);

            return Ok(new
            {
                success = true,
                fileName = fileName,
                bucket = bucketName,
                contentType = file.ContentType,
                size = file.Length,
                url = url
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "Failed to upload file" });
        }
    }

    [HttpPost("upload-multiple")]
    public async Task<IActionResult> UploadMultipleFiles(
        List<IFormFile> files,
        [FromQuery] string category = "general")
    {
        if (files == null || files.Count == 0)
        {
            return BadRequest(new { error = "At least one file is required" });
        }

        var results = new List<object>();
        var errors = new List<string>();

        foreach (var file in files)
        {
            try
            {
                string bucketName = DetermineBucket(file, category);

                using var stream = file.OpenReadStream();
                var fileName = await _storageService.UploadFileAsync(
                    stream,
                    file.FileName,
                    bucketName,
                    file.ContentType);

                results.Add(new
                {
                    originalName = file.FileName,
                    storedName = fileName,
                    bucket = bucketName,
                    success = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {file.FileName}");
                errors.Add($"Failed to upload {file.FileName}");
            }
        }

        return Ok(new
        {
            uploaded = results.Count,
            failed = errors.Count,
            files = results,
            errors = errors
        });
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadFile(
        string fileName,
        [FromQuery] string bucket)
    {
        try
        {
            var stream = await _storageService.DownloadFileAsync(fileName, bucket);

            // Determine content type based on file extension
            var contentType = GetContentType(fileName);

            return File(stream, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file {fileName}");
            return NotFound(new { error = "File not found" });
        }
    }

    [HttpGet("url/{fileName}")]
    public async Task<IActionResult> GetPresignedUrl(
        string fileName,
        [FromQuery] string bucket,
        [FromQuery] int expirationMinutes = 60)
    {
        try
        {
            var url = await _storageService.GetPresignedUrlAsync(
                fileName,
                bucket,
                expirationMinutes);

            return Ok(new
            {
                fileName = fileName,
                url = url,
                expiresInMinutes = expirationMinutes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating presigned URL for {fileName}");
            return StatusCode(500, new { error = "Failed to generate URL" });
        }
    }

    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteFile(
        string fileName,
        [FromQuery] string bucket)
    {
        try
        {
            var result = await _storageService.DeleteFileAsync(fileName, bucket);

            if (result)
            {
                return Ok(new { success = true, message = "File deleted successfully" });
            }

            return NotFound(new { error = "File not found or could not be deleted" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file {fileName}");
            return StatusCode(500, new { error = "Failed to delete file" });
        }
    }

    [HttpGet("list")]
    public async Task<IActionResult> ListFiles(
        [FromQuery] string bucket,
        [FromQuery] string prefix = null)
    {
        try
        {
            var files = await _storageService.ListFilesAsync(bucket, prefix);

            var fileList = files.Select(f => new
            {
                key = f.Key,
                size = f.Size,
                lastModified = f.LastModified,
                bucket = bucket
            }).ToList();

            return Ok(new
            {
                count = fileList.Count,
                files = fileList
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error listing files in bucket {bucket}");
            return StatusCode(500, new { error = "Failed to list files" });
        }
    }

    private string DetermineBucket(IFormFile file, string category)
    {
        // Logic to determine bucket based on file type or category
        if (IsImage(file.ContentType))
        {
            return _config.BucketNames.Images;
        }
        else if (IsDocument(file.ContentType))
        {
            return _config.BucketNames.Documents;
        }
        else
        {
            return _config.BucketNames.TempFiles;
        }
    }

    private bool IsImage(string contentType)
    {
        var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp", "image/svg+xml" };
        return imageTypes.Contains(contentType.ToLower());
    }

    private bool IsDocument(string contentType)
    {
        var docTypes = new[]
        {
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        return docTypes.Contains(contentType.ToLower());
    }

    private string GetContentType(string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}

// DTO for file upload response
public class FileUploadResponse
{
    public bool Success { get; set; }
    public string FileName { get; set; }
    public string Bucket { get; set; }
    public string Url { get; set; }
    public long Size { get; set; }
}