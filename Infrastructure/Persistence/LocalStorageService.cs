using Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistence;

public class LocalStorageService(IWebHostEnvironment webHostEnvironment) : IStorageService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg", ".pdf"
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct)
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

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, ct);
        }

        return $"/uploads/{folder}/{fileName}".Replace("\\", "/");
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
}
