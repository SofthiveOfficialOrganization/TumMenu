using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WebUI.Seo;

namespace WebUI.IntegrationTests.Unit.Seo;

public sealed class PublicSeoTests
{
    [Fact]
    public void GetCanonicalBaseUrl_UsesConfiguredCanonicalBase()
    {
        var context = CreateContext("https://example.com/");

        PublicSeo.GetCanonicalBaseUrl(context).Should().Be("https://example.com");
    }

    [Fact]
    public void GetCanonicalBaseUrl_FallsBackToTummenuWhenConfigIsMissing()
    {
        var context = CreateContext(canonicalBaseUrl: null);

        PublicSeo.GetCanonicalBaseUrl(context).Should().Be("https://tummenu.com");
    }

    [Fact]
    public void ToCanonicalUrl_IgnoresRequestHost()
    {
        var context = CreateContext("https://tummenu.com");
        context.Request.Scheme = "http";
        context.Request.Host = new HostString("www.tummenu.com");

        var result = PublicSeo.ToCanonicalUrl(context.Request, "/qr-kod", QueryString.Create("utm_source", "test"));

        result.Should().Be("https://tummenu.com/qr-kod?utm_source=test");
    }

    [Fact]
    public void ToAbsoluteUrl_UsesCanonicalBaseForRelativePath()
    {
        var context = CreateContext("https://tummenu.com");

        var result = PublicSeo.ToAbsoluteUrl(context.Request, "/images/og.webp");

        result.Should().Be("https://tummenu.com/images/og.webp");
    }

    [Fact]
    public void ResolveOgImage_UsesCanonicalBaseForDefaultImage()
    {
        var context = CreateContext("https://tummenu.com");

        var result = PublicSeo.ResolveOgImage(context.Request, viewDataOgImage: null);

        result.Should().Be("https://tummenu.com/images/tum-menu-logo-safe.webp");
    }

    private static DefaultHttpContext CreateContext(string? canonicalBaseUrl)
    {
        var values = new Dictionary<string, string?>();
        if (canonicalBaseUrl is not null)
        {
            values["Seo:CanonicalBaseUrl"] = canonicalBaseUrl;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .BuildServiceProvider();

        return new DefaultHttpContext { RequestServices = services };
    }
}
