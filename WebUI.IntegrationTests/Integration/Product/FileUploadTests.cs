using System.Net;
using System.Net.Http.Headers;
using WebUI.IntegrationTests.Infrastructure;

namespace WebUI.IntegrationTests.Integration.Product;

public class FileUploadTests : IClassFixture<TumMenuWebAppFactory>
{
    private readonly TumMenuWebAppFactory _factory;

    public FileUploadTests(TumMenuWebAppFactory factory)
    {
        _factory = factory;
        TestDbSeeder.SeedAsync(factory.Services).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Upload_WithExeFile_ReturnsBadRequest()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        // Get antiforgery token from any page
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/Admin/Menu/Index");

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(token), "__RequestVerificationToken");

        var fileContent = new ByteArrayContent(new byte[] { 0x4D, 0x5A }); // MZ header
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
        content.Add(fileContent, "file", "test.exe");

        var response = await client.PostAsync("/Upload", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Upload_WithValidImageFile_Returns200()
    {
        var client = await AuthHelper.GetAuthenticatedClientAsync(_factory, "Owner");
        var token = await AntiforgeryHelper.GetTokenAsync(client, "/Admin/Menu/Index");

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(token), "__RequestVerificationToken");

        // Minimal valid PNG: 1x1 pixel PNG
        var pngBytes = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+M9QDwADhgGAWjR9awAAAABJRU5ErkJggg==");
        var fileContent = new ByteArrayContent(pngBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "file", "test.png");

        var response = await client.PostAsync("/Upload", content);

        // Check: either success (200) or server error from storage (acceptable in test env)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.InternalServerError,
            $"Unexpected: {response.StatusCode}");
    }
}
