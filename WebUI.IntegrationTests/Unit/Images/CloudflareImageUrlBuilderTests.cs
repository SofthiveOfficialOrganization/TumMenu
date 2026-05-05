using FluentAssertions;
using Microsoft.AspNetCore.Http;
using WebUI.Services;

namespace WebUI.IntegrationTests.Unit.Images;

public sealed class CloudflareImageUrlBuilderTests
{
    private readonly CloudflareImageUrlBuilder _builder = new();

    [Fact]
    public void Build_TransformsUploadsImageUrl()
    {
        var result = _builder.Build("/uploads/product/a.webp");

        result.Should().Be("/cdn-cgi/image/format=auto,quality=75,metadata=none/uploads/product/a.webp");
    }

    [Fact]
    public void Build_DoesNotTransformWhenRequestDidNotComeThroughCloudflare()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("tummenu.com");
        var builder = new CloudflareImageUrlBuilder(new HttpContextAccessor { HttpContext = context });

        builder.Build("/uploads/product/a.webp").Should().Be("/uploads/product/a.webp");
    }

    [Fact]
    public void Build_TransformsWhenRequestCameThroughCloudflare()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("tummenu.com");
        context.Request.Headers["CF-Ray"] = "test-ray";
        var builder = new CloudflareImageUrlBuilder(new HttpContextAccessor { HttpContext = context });

        builder.Build("/uploads/product/a.webp")
            .Should().Be("/cdn-cgi/image/format=auto,quality=75,metadata=none/uploads/product/a.webp");
    }

    [Fact]
    public void Build_TransformsWithWidthAndHeight()
    {
        var context = new DefaultHttpContext();
        context.Request.Host = new HostString("tummenu.com");
        context.Request.Headers["CF-Ray"] = "test-ray";
        var builder = new CloudflareImageUrlBuilder(new HttpContextAccessor { HttpContext = context });

        builder.Build("/uploads/product/a.webp", width: 800)
            .Should().Be("/cdn-cgi/image/format=auto,quality=75,metadata=none,width=800/uploads/product/a.webp");
            
        builder.Build("/uploads/product/a.webp", width: 800, height: 600)
            .Should().Be("/cdn-cgi/image/format=auto,quality=75,metadata=none,width=800,height=600/uploads/product/a.webp");
    }

    [Theory]
    [InlineData("https://cdn.example.com/a.jpg")]
    [InlineData("/uploads/product/icon.svg")]
    [InlineData("/uploads/product/doc.pdf")]
    [InlineData("/uploads/product/anim.gif")]
    [InlineData("/cdn-cgi/image/format=auto,quality=75,metadata=none/uploads/product/a.webp")]
    public void Build_DoesNotTransformUnsupportedOrAlreadyTransformedUrls(string url)
    {
        _builder.Build(url).Should().Be(url);
    }
}
