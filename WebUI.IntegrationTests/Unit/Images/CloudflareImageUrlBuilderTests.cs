using FluentAssertions;
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
