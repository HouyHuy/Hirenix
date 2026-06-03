namespace Hirenix.Application.Interfaces;

/// <summary>
/// Service for handling file storage operations
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Save a file to storage
    /// </summary>
    /// <param name="fileStream">The file stream to save</param>
    /// <param name="fileName">Original filename</param>
    /// <param name="folder">Folder to save in (e.g., "cvs", "avatars")</param>
    /// <returns>The URL/path to the saved file</returns>
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder);

    /// <summary>
    /// Delete a file from storage
    /// </summary>
    /// <param name="fileUrl">The URL/path of the file to delete</param>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Check if a file exists
    /// </summary>
    /// <param name="fileUrl">The URL/path to check</param>
    /// <returns>True if file exists</returns>
    Task<bool> FileExistsAsync(string fileUrl);

    /// <summary>
    /// Get the full path to a file
    /// </summary>
    /// <param name="fileUrl">The relative URL/path</param>
    /// <returns>Full file system path</returns>
    string GetFullPath(string fileUrl);

    /// <summary>
    /// Get a client-accessible URL for reading the file.
    /// Local storage returns the relative URL, cloud providers return signed URLs.
    /// </summary>
    /// <param name="fileUrl">Stored file reference</param>
    /// <returns>URL that clients can use to access file content</returns>
    string GetAccessUrl(string fileUrl);
}
