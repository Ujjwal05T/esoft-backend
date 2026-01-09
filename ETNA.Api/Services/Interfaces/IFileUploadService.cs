namespace ETNA.Api.Services.Interfaces;

/// <summary>
/// Service interface for file upload operations
/// </summary>
public interface IFileUploadService
{
    /// <summary>
    /// Upload a file and return the URL/path
    /// </summary>
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder);
    
    /// <summary>
    /// Delete a file by its URL/path
    /// </summary>
    Task<bool> DeleteFileAsync(string fileUrl);
    
    /// <summary>
    /// Validate if file type is allowed
    /// </summary>
    bool IsValidImageFile(string fileName);
}
