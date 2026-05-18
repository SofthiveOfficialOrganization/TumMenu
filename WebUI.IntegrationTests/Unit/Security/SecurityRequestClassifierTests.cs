using FluentAssertions;
using Microsoft.AspNetCore.Http;
using WebUI.Security;

namespace WebUI.IntegrationTests.Unit.Security;

public class SecurityRequestClassifierTests
{
    [Theory]
    [InlineData("/.git/config")]
    [InlineData("/backend/.env")]
    [InlineData("/backup.sql")]
    public void IsKnownSecurityProbe_SuspiciousPaths_ReturnsTrue(string path)
    {
        SecurityRequestClassifier.IsKnownSecurityProbe(new PathString(path))
            .Should()
            .BeTrue();
    }

    [Theory]
    [InlineData("/restoranlar")]
    [InlineData("/hakkimizda")]
    [InlineData("/oren-kebap/merkez-sube")]
    [InlineData("/.well-known/appspecific/com.chrome.devtools.json")]
    public void ClassifySuccessfulRequest_NormalPublicPaths_ReturnsNone(string path)
    {
        var context = CreateContext(path);

        var result = SecurityRequestClassifier.ClassifySuccessfulRequest(context);

        result.Kind.Should().Be(SecurityRequestKind.None);
    }

    [Theory]
    [InlineData("/api/stores/search")]
    [InlineData("/api/categories/search")]
    [InlineData("/api/location/provinces")]
    [InlineData("/api/auth/login")]
    public void ClassifySuccessfulRequest_AllowedApiPaths_ReturnsNone(string path)
    {
        var context = CreateContext(path);

        var result = SecurityRequestClassifier.ClassifySuccessfulRequest(context);

        result.Kind.Should().Be(SecurityRequestKind.None);
    }

    [Theory]
    [InlineData("/Admin")]
    [InlineData("/Admin/Dashboard")]
    [InlineData("/admin/store")]
    public void ClassifySuccessfulRequest_AdminPaths_ReturnsNone(string path)
    {
        var context = CreateContext(path);

        var result = SecurityRequestClassifier.ClassifySuccessfulRequest(context);

        result.Kind.Should().Be(SecurityRequestKind.None);
    }

    [Fact]
    public void ClassifySuccessfulRequest_UnexpectedApiSuccess_ReturnsUnexpectedSensitiveSuccess()
    {
        var context = CreateContext("/api/debug");

        var result = SecurityRequestClassifier.ClassifySuccessfulRequest(context);

        result.Kind.Should().Be(SecurityRequestKind.UnexpectedSensitiveSuccess);
        result.ErrorCode.Should().Be("unexpected_sensitive_success");
    }

    private static DefaultHttpContext CreateContext(string path)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        return context;
    }
}
