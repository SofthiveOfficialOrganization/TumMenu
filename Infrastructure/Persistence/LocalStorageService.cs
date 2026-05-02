using Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using ImageMagick;

namespace Infrastructure.Persistence;

public class LocalStorageService(IWebHostEnvironment webHostEnvironment) : IStorageService
{
    private const uint OptimizedWebpQuality = 75;

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".pdf"
    };

    private static readonly HashSet<string> OptimizableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<StorageUploadResult> UploadAsync(IFormFile file, string folder, CancellationToken ct)
    {
        var extension = Path.GetExtension(file.FileName);

        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Desteklenmeyen dosya türü: {extension}. İzin verilenler: {string.Join(", ", AllowedExtensions)}");

        if (file.Length > MaxFileSizeBytes)
            throw new InvalidOperationException($"Dosya boyutu çok büyük. Maksimum izin verilen: {MaxFileSizeBytes / 1024 / 1024} MB.");

        var wwwrootPath = webHostEnvironment.WebRootPath;
        var uploadFolder = Path.Combine(wwwrootPath, "uploads", folder);

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var shouldOptimize = OptimizableExtensions.Contains(extension);
        var savedExtension = shouldOptimize ? ".webp" : extension.ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{savedExtension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        int? width = null;
        int? height = null;

        if (shouldOptimize)
        {
            using var inputStream = file.OpenReadStream();
            using var image = new MagickImage(inputStream);
            image.AutoOrient();
            width = (int)image.Width;
            height = (int)image.Height;
            image.Strip();
            image.Format = MagickFormat.WebP;
            image.Quality = OptimizedWebpQuality;
            await image.WriteAsync(filePath, ct);
        }
        else
        {
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream, ct);
        }

        var savedFile = new FileInfo(filePath);
        var mimeType = shouldOptimize ? "image/webp" : GetMimeType(savedExtension, file.ContentType);
        var url = $"/uploads/{folder}/{fileName}".Replace("\\", "/");

        return new StorageUploadResult(url, width, height, savedFile.Length, savedExtension, mimeType);
    }

    public Task DeleteAsync(string path)
    {
        if (string.IsNullOrEmpty(path)) return Task.CompletedTask;

        var wwwrootPath = webHostEnvironment.WebRootPath;
        // path usually starts with /uploads/...
        var relativePath = path.TrimStart('/');
        var fullPath = Path.Combine(wwwrootPath, relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }

        return Task.CompletedTask;
    }

    private static string GetMimeType(string extension, string fallback) => extension.ToLowerInvariant() switch
    {
        ".svg" => "image/svg+xml",
        ".gif" => "image/gif",
        ".pdf" => "application/pdf",
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => string.IsNullOrWhiteSpace(fallback) ? "application/octet-stream" : fallback
    };
}
