using Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace WebUI.IntegrationTests.Infrastructure;

public class TumMenuWebAppFactory : WebApplicationFactory<Program>
{
    public const string TestConnectionString =
        "Server=(localdb)\\MSSQLLocalDB;Database=TumMenuDb_Dev;Trusted_Connection=True;TrustServerCertificate=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Replace the DbContext registration to point to TumMenuDb_Dev
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(TestConnectionString));

            // Replace CloudflareTurnstile with a stub that always passes
            services.RemoveAll<WebUI.Services.Turnstile.ITurnstileService>();
            services.AddSingleton<WebUI.Services.Turnstile.ITurnstileService,
                AlwaysPassTurnstileService>();
        });
    }
}

// Stub: Turnstile always succeeds in tests
public class AlwaysPassTurnstileService : WebUI.Services.Turnstile.ITurnstileService
{
    public Task<WebUI.Models.TurnstileValidationResult> ValidateAsync(string? token)
        => Task.FromResult(new WebUI.Models.TurnstileValidationResult { Success = true });
}
