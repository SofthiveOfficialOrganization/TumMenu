using FluentAssertions;
using ImageMagick;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace WebUI.IntegrationTests.Unit.Images;

public sealed class LocalStorageServiceTests
{
    [Fact]
    public async Task UploadAsync_OptimizesPngAsWebpWithoutResizing()
    {
        var webRoot = CreateTempDirectory();
        try
        {
            var file = CreateRasterFormFile("source.png", MagickFormat.Png, 17, 11);
            var service = new LocalStorageService(new TestWebHostEnvironment(webRoot));

            var result = await service.UploadAsync(file, "product", CancellationToken.None);

            result.Extension.Should().Be(".webp");
            result.MimeType.Should().Be("image/webp");
            result.Width.Should().Be(17);
            result.Height.Should().Be(11);
            result.FileSize.Should().BeGreaterThan(0);

            var savedPath = Path.Combine(webRoot, result.Url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            File.Exists(savedPath).Should().BeTrue();

            using var savedImage = new MagickImage(savedPath);
            savedImage.Width.Should().Be(17);
            savedImage.Height.Should().Be(11);
            savedImage.Format.Should().Be(MagickFormat.WebP);
            savedImage.GetAttribute("comment").Should().BeNullOrEmpty();
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    [Fact]
    public async Task UploadAsync_KeepsSvgAsPassThrough()
    {
        var webRoot = CreateTempDirectory();
        try
        {
            var svg = """
                <svg xmlns="http://www.w3.org/2000/svg" width="10" height="8"><rect width="10" height="8" fill="red"/></svg>
                """;
            var file = CreateFormFile("icon.svg", "image/svg+xml", svg);
            var service = new LocalStorageService(new TestWebHostEnvironment(webRoot));

            var result = await service.UploadAsync(file, "product", CancellationToken.None);

            result.Extension.Should().Be(".svg");
            result.MimeType.Should().Be("image/svg+xml");
            result.Width.Should().BeNull();
            result.Height.Should().BeNull();
            result.Url.Should().EndWith(".svg");
        }
        finally
        {
            Directory.Delete(webRoot, recursive: true);
        }
    }

    private static IFormFile CreateRasterFormFile(string fileName, MagickFormat format, uint width, uint height)
    {
        var stream = new MemoryStream();
        using (var image = new MagickImage(MagickColors.Red, width, height))
        {
            image.Format = format;
            image.SetAttribute("comment", "remove me");
            image.Write(stream);
        }

        stream.Position = 0;
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = format == MagickFormat.Png ? "image/png" : "image/jpeg"
        };
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, string content)
    {
        var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
        return new FormFile(stream, 0, stream.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    private static string CreateTempDirectory()
    {
        var path = Path.Combine(Path.GetTempPath(), "tummenu-storage-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(path);
        return path;
    }

    private sealed class TestWebHostEnvironment(string webRootPath) : IWebHostEnvironment
    {
        public string WebRootPath { get; set; } = webRootPath;
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ApplicationName { get; set; } = "TumMenu.Tests";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = webRootPath;
        public string EnvironmentName { get; set; } = Environments.Development;
    }
}
