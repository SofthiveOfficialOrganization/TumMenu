using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebUI.IntegrationTests.Infrastructure;

public class TumMenuWebAppFactory : WebApplicationFactory<Program>
{
    private const string FallbackTestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=TumMenuDb_Dev;Trusted_Connection=True;TrustServerCertificate=True";

    public static string TestConnectionString =>
        FirstNonEmpty(
            Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING"),
            Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection"))
        ?? FallbackTestConnectionString;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Override AllowedHosts so host-filtering middleware accepts test requests
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AllowedHosts"] = "*"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Replace the DbContext registration to point to the isolated test database.
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(TestConnectionString));

            // Replace CloudflareTurnstile with a stub that always passes
            services.RemoveAll<WebUI.Services.Turnstile.ITurnstileService>();
            services.AddSingleton<WebUI.Services.Turnstile.ITurnstileService,
                AlwaysPassTurnstileService>();

            // Replace IEmailSender with a no-op stub to prevent SMTP failures
            services.RemoveAll<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender>();
            services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender,
                NoOpEmailSender>();
        });
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        return values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
    }
}

// Stub: Email sender that does nothing in tests
public class NoOpEmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
        => Task.CompletedTask;
}

// Stub: Turnstile always succeeds in tests
public class AlwaysPassTurnstileService : WebUI.Services.Turnstile.ITurnstileService
{
    public Task<WebUI.Models.TurnstileValidationResult> ValidateAsync(string? token)
        => Task.FromResult(new WebUI.Models.TurnstileValidationResult { Success = true });
}
