using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Hirenix.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Hirenix.Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".pdf", ".doc", ".docx" };

    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly string _basePath;
    private readonly int _signedUrlMinutes;

    public S3FileStorageService(IConfiguration configuration)
    {
        var section = configuration.GetSection("Storage:S3");
        _bucketName = section["BucketName"] ?? string.Empty;
        _basePath = section["BasePath"] ?? "hirenix";
        _signedUrlMinutes = int.TryParse(section["SignedUrlMinutes"], out var minutes) ? minutes : 30;

        if (string.IsNullOrWhiteSpace(_bucketName))
        {
            throw new InvalidOperationException("Storage:S3:BucketName is required when using S3 file storage.");
        }

        var regionName = section["Region"] ?? "ap-southeast-1";
        var accessKey = section["AccessKey"];
        var secretKey = section["SecretKey"];

        if (!string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(secretKey))
        {
            _s3Client = new AmazonS3Client(accessKey, secretKey, RegionEndpoint.GetBySystemName(regionName));
        }
        else
        {
            _s3Client = new AmazonS3Client(RegionEndpoint.GetBySystemName(regionName));
        }
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
        var key = BuildObjectKey(folder, extension);

        await _s3Client.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = payload,
            ContentType = GetContentType(extension)
        });

        return $"s3://{_bucketName}/{key}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        var key = ParseObjectKey(fileUrl);
        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        await _s3Client.DeleteObjectAsync(new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        });
    }

    public async Task<bool> FileExistsAsync(string fileUrl)
    {
        var key = ParseObjectKey(fileUrl);
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        try
        {
            await _s3Client.GetObjectMetadataAsync(_bucketName, key);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public string GetFullPath(string fileUrl)
    {
        return ParseObjectKey(fileUrl);
    }

    public string GetAccessUrl(string fileUrl)
    {
        var key = ParseObjectKey(fileUrl);
        if (string.IsNullOrWhiteSpace(key))
        {
            return fileUrl;
        }

        return _s3Client.GetPreSignedURL(new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Expires = DateTime.UtcNow.AddMinutes(_signedUrlMinutes)
        });
    }

    private static void ValidateExtension(string extension)
    {
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException($"File type {extension} is not allowed. Only PDF, DOC, and DOCX files are accepted.");
        }
    }

    private string BuildObjectKey(string folder, string extension)
    {
        var normalizedBase = _basePath.Trim('/');
        var normalizedFolder = folder.Trim('/');
        var name = $"{Guid.NewGuid()}{extension}";
        return $"{normalizedBase}/{normalizedFolder}/{name}";
    }

    private static string GetContentType(string extension) => extension switch
    {
        ".pdf" => "application/pdf",
        ".doc" => "application/msword",
        ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        _ => "application/octet-stream"
    };

    private string ParseObjectKey(string fileUrl)
    {
        if (string.IsNullOrWhiteSpace(fileUrl))
        {
            return string.Empty;
        }

        const string prefix = "s3://";
        if (fileUrl.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
        {
            var withoutPrefix = fileUrl[prefix.Length..];
            var firstSlash = withoutPrefix.IndexOf('/');
            if (firstSlash < 0)
            {
                return string.Empty;
            }

            var bucket = withoutPrefix[..firstSlash];
            var key = withoutPrefix[(firstSlash + 1)..];
            return string.Equals(bucket, _bucketName, StringComparison.OrdinalIgnoreCase) ? key : string.Empty;
        }

        return fileUrl.TrimStart('/');
    }
}
