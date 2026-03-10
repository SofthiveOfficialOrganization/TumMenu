using Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Persistence;

public class LocalStorageService(IWebHostEnvironment webHostEnvironment) : IStorageService
{
    public async Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct)
    {
        var wwwrootPath = webHostEnvironment.WebRootPath;
        var uploadFolder = Path.Combine(wwwrootPath, "uploads", folder);

        if (!Directory.Exists(uploadFolder))
        {
            Directory.CreateDirectory(uploadFolder);
        }

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
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
