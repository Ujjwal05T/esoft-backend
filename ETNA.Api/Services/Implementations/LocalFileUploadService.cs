using ETNA.Api.Services.Interfaces;

namespace ETNA.Api.Services.Implementations;

/// <summary>
/// Local file system upload service implementation
/// For production, consider using Azure Blob Storage or AWS S3
/// </summary>
public class LocalFileUploadService : IFileUploadService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<LocalFileUploadService> _logger;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    private const long MaxFileSize = 10 * 1024 * 1024; // 10MB
    
    public LocalFileUploadService(
        IWebHostEnvironment environment,
        ILogger<LocalFileUploadService> logger)
    {
        _environment = environment;
        _logger = logger;
    }
    
    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder)
    {
        try
        {
            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_environment.ContentRootPath, "uploads", folder);
            Directory.CreateDirectory(uploadsPath);
            
            // Generate unique filename
            var fileExtension = Path.GetExtension(fileName).ToLower();
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, uniqueFileName);
            
            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(stream);
            }
            
            // Return relative URL
            var relativeUrl = $"/uploads/{folder}/{uniqueFileName}";
            
            _logger.LogInformation("File uploaded successfully: {FilePath}", relativeUrl);
            
            return relativeUrl;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", fileName);
            throw;
        }
    }
    
    public Task<bool> DeleteFileAsync(string fileUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(fileUrl))
                return Task.FromResult(false);
            
            // Convert URL to file path
            var relativePath = fileUrl.TrimStart('/');
            var filePath = Path.Combine(_environment.ContentRootPath, relativePath);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("File deleted: {FilePath}", filePath);
                return Task.FromResult(true);
            }
            
            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileUrl}", fileUrl);
            return Task.FromResult(false);
        }
    }
    
    public bool IsValidImageFile(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;
        
        var extension = Path.GetExtension(fileName).ToLower();
        return _allowedExtensions.Contains(extension);
    }
}
