using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using WebUI.Models;
using WebUI.Services.Turnstile;

namespace WebUI.Tests.Services;

public class TurnstileServiceTests
{
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
    private readonly Mock<IConfiguration> _mockConfiguration;

    public TurnstileServiceTests()
    {
        _mockHttpClientFactory = new Mock<IHttpClientFactory>();
        _mockConfiguration = new Mock<IConfiguration>();
    }


    private IConfigurationSection CreateConfigurationSection(string secretKey, bool enabled)
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "CloudflareTurnstile:SecretKey", secretKey },
            { "CloudflareTurnstile:Enabled", enabled.ToString() }
        };

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        return config.GetSection("CloudflareTurnstile");
    }

    [Fact]
    public async Task ValidateAsync_WithNullToken_ReturnsFalse()
    {
        // Arrange
        var turnstileSection = CreateConfigurationSection("test-secret", true);
        _mockConfiguration.Setup(x => x.GetSection("CloudflareTurnstile")).Returns(turnstileSection);

        var service = new TurnstileService(_mockHttpClientFactory.Object, _mockConfiguration.Object);

        // Act
        var result = await service.ValidateAsync(null);

        // Assert
        Assert.False(result.Success);
        Assert.Contains("missing-token", result.ErrorCodes);
    }

    [Fact]
    public async Task ValidateAsync_WhenDisabled_ReturnsSuccess()
    {
        // Arrange
        var turnstileSection = CreateConfigurationSection("test-secret", false);
        _mockConfiguration.Setup(x => x.GetSection("CloudflareTurnstile")).Returns(turnstileSection);

        var service = new TurnstileService(_mockHttpClientFactory.Object, _mockConfiguration.Object);

        // Act
        var result = await service.ValidateAsync("any-token");

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ValidateAsync_WithValidToken_CallsCloudflareAPI()
    {
        // Arrange
        var turnstileSection = CreateConfigurationSection("test-secret-key", true);
        _mockConfiguration.Setup(x => x.GetSection("CloudflareTurnstile")).Returns(turnstileSection);

        var mockResponse = new HttpResponseMessage
        {
            StatusCode = System.Net.HttpStatusCode.OK,
            Content = new StringContent("""{"success": true, "challenge_ts": "2026-04-03T12:00:00Z", "hostname": "example.com"}""")
        };

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(mockResponse);

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new TurnstileService(_mockHttpClientFactory.Object, _mockConfiguration.Object);

        // Act
        var result = await service.ValidateAsync("valid-token");

        // Assert
        Assert.True(result.Success);
    }

    [Fact]
    public async Task ValidateAsync_WhenAPIFails_ReturnsFalse()
    {
        // Arrange
        var turnstileSection = CreateConfigurationSection("test-secret", true);
        _mockConfiguration.Setup(x => x.GetSection("CloudflareTurnstile")).Returns(turnstileSection);

        var mockHandler = new Mock<HttpMessageHandler>();
        mockHandler.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Connection failed"));

        var httpClient = new HttpClient(mockHandler.Object);
        _mockHttpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var service = new TurnstileService(_mockHttpClientFactory.Object, _mockConfiguration.Object);

        // Act
        var result = await service.ValidateAsync("valid-token");

        // Assert
        Assert.False(result.Success);
        Assert.Contains("validation-error", result.ErrorCodes);
    }
}
