using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Hirenix.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Hirenix.Infrastructure.Services;

public class AzureBlobFileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };

    private readonly BlobContainerClient _containerClient;
    private readonly string _basePath;
    private readonly int _signedUrlMinutes;

    public AzureBlobFileStorageService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage:AzureBlob");
        var connectionString = section["ConnectionString"] ?? string.Empty;
        var container = section["ContainerName"] ?? "hirenix";
        _basePath = section["BasePath"] ?? "hirenix";
        _signedUrlMinutes = int.TryParse(section["SignedUrlMinutes"], out var minutes) ? minutes : 30;

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Storage:AzureBlob:ConnectionString is required when using Azure Blob file storage.");
        }

        _containerClient = new BlobContainerClient(connectionString, container);
        _containerClient.CreateIfNotExists();
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        ValidateExtension(extension);

        await using var payload = new MemoryStream();
        await fileStream.CopyToAsync(payload);

        if (payload.Length > MaxFileSizeBytes)
        {
            throw new InvalidOperationException($"File size exceeds maximum allowed size of {MaxFileSizeBytes / 1024 / 1024}MB.");
        }

        payload.Position = 0;
        var blobName = BuildBlobName(folder, extension);
        var blobClient = _containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(payload, overwrite: false);
        return $"azureblob://{_containerClient.Name}/{blobName}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        var blobName = ParseBlobName(fileUrl);
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.DeleteIfExistsAsync();
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        var blobName = ParseBlobName(fileUrl);
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return false;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        return await blobClient.ExistsAsync();
    }

    public string GetFullPath(string fileUrl)
    {
        return ParseBlobName(fileUrl);
    }

    public string GetAccessUrl(string fileUrl)
    {
        var blobName = ParseBlobName(fileUrl);
        if (string.IsNullOrWhiteSpace(blobName))
        {
            return fileUrl;
        }

        var blobClient = _containerClient.GetBlobClient(blobName);
        if (!blobClient.CanGenerateSasUri)
        {
            return blobClient.Uri.ToString();
        }

        var sas = blobClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddMinutes(_signedUrlMinutes));
        return sas.ToString();
    }

    private static void ValidateExtension(string extension)
    {
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type {extension} is not allowed. Only PDF, DOC, and DOCX files are accepted.");
        }
    }

    private string BuildBlobName(string folder, string extension)
    {
        var normalizedBase = _basePath.Trim('/');
        var normalizedFolder = folder.Trim('/');
        var fileName = $"{Guid.NewGuid()}{extension}";
        return $"{normalizedBase}/{normalizedFolder}/{fileName}";
    }

    private string ParseBlobName(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return string.Empty;
        }

        const string prefix = "azureblob://";
        if (fileUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var withoutPrefix = fileUrl[prefix.Length..];
            var slashIndex = withoutPrefix.IndexOf('/');
            if (slashIndex < 0)
            {
                return string.Empty;
            }

            var container = withoutPrefix[..slashIndex];
            var blobName = withoutPrefix[(slashIndex + 1)..];
            return string.Equals(container, _containerClient.Name, StringComparison.OrdinalIgnoreCase) ? blobName : string.Empty;
        }

        return fileUrl.TrimStart('/');
    }
}
