using Hirenix.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;

namespace Hirenix.Infrastructure.Services;

/// <summary>
/// Local file system storage implementation
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadBasePath;
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };

    public LocalFileStorageService(IWebHostEnvironment environment)
    {
        // Store files in wwwroot/uploads folder
        var webRoot = environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        _uploadBasePath = Path.Combine(webRoot, "uploads");
        
        // Ensure uploads directory exists
        if (!Directory.Exists(_uploadBasePath))
        {
            Directory.CreateDirectory(_uploadBasePath);
        }
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        // Validate file extension
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type {extension} is not allowed. Only PDF, DOC, and DOCX files are accepted.");
        }

        // Validate file size
        if (fileStream.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024}MB.");
        }

        // Create folder if it doesn't exist
        var folderPath = Path.Combine(_uploadBasePath, folder);
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        // Generate unique filename to avoid conflicts
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, uniqueFileName);

        // Save file
        using (var fileStreamOutput = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fileStreamOutput);
        }

        // Return relative URL path
        return $"/uploads/{folder}/{uniqueFileName}";
    }

    public Task DeleteFileAsync(string fileUrl)
    {
        try
        {
            var fullPath = GetFullPath(fileUrl);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }
        catch (Exception ex)
        {
            // Log error but don't throw - file deletion is not critical
            Console.WriteLine($"Error deleting file {fileUrl}: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public Task<bool> FileExistsAsync(string fileUrl)
    {
        var fullPath = GetFullPath(fileUrl);
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetFullPath(string fileUrl)
    {
        // Remove leading slash if present
        var relativePath = fileUrl.TrimStart('/');
        
        // Convert URL path to file system path
        relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        
        // Combine with base path
        return Path.Combine(_uploadBasePath, "..", relativePath);
    }

    public string GetAccessUrl(string fileUrl)
    {
        return fileUrl;
    }
}
