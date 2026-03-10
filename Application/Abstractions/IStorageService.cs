using Microsoft.AspNetCore.Http;

namespace Application.Abstractions;

public interface IStorageService
{
    Task<string> UploadAsync(IFormFile file, string folder, CancellationToken ct);
    Task DeleteAsync(string path);
}
