using Microsoft.AspNetCore.Http;

namespace Application.Abstractions;

public interface IStorageService
{
    Task<StorageUploadResult> UploadAsync(IFormFile file, string folder, CancellationToken ct);
    Task DeleteAsync(string path);
}

public sealed record StorageUploadResult(
    string Url,
    int? Width,
    int? Height,
    long FileSize,
    string Extension,
    string MimeType);
