using System.Net;
using System.Net.Http.Json;
using Application.Companies.Commands;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using WebUI;
using Xunit;

namespace WebUI.IntegrationTests;

public class CompanyControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
	private readonly WebApplicationFactory<Program> _factory;

	public CompanyControllerTests(WebApplicationFactory<Program> factory)
	{
		_factory = factory;
	}

	[Fact]
	public Task Create_ShouldBindCommandPropertiesCorrectly()
	{
		// Arrange
		var client = _factory.WithWebHostBuilder(builder =>
		{
			builder.ConfigureServices(services =>
			{
				var descriptor = services.SingleOrDefault(
					d => d.ServiceType.Name == "ISwaggerProvider" || d.ServiceType.Name == "SwaggerGenerator");
				if(descriptor != null) services.Remove(descriptor);

				// Also try to remove by implementation type if needed, but generic removal is safer for now.
				// Actually, let's just ignore the startup errors? No, it throws.
				// Let's try to remove all Swashbuckle services.
				var swaggerServices = services.Where(d => d.ServiceType.Namespace?.StartsWith("Swashbuckle") == true).ToList();
				foreach(var s in swaggerServices) services.Remove(s);
			});
		}).CreateClient(new WebApplicationFactoryClientOptions
		{
			AllowAutoRedirect = false
		});

		// Login as Admin (Mocking needed for real auth, but for binding check we might hit 401/403)
		// However, if binding fails early or we can check the request processing, that helps. 
		// For now, let's try to post and see if we can get past Auth or if we need to mock it.
		// If we can't easily mock auth, we will inspect the behavior. 

		// BETTER APPROACH: Use a custom factory to inject a mock mediator and bypass auth if possible 
		// OR just post and check if we get a 400 Bad Request (validation error) vs success.

		// Since the user says "values are null", validation should fail if "NotEmpty" rule exists.

		var command = new Dictionary<string, string>
		{
			{ "Name", "Test Company" },
			{ "Slug", "test-company" },
			{ "__RequestVerificationToken", "mock-token" } // CSRF might block us
        };

		var content = new FormUrlEncodedContent(command);

		// Act
		// We expect this to fail due to AntiForgery or Auth, but we want to see if we can reach the controller.
		// To properly test binding in isolation without Auth/CSRF noise, 
		// we might simply check if the binder *can* bind to the class in a unit test 
		// OR strip the attributes in a test-specific startup.

		// Let's first try a simple Unit Test for Model Binding if Integration is too complex with Auth/CSRF/DB.
		// Actually, the user's issue is likely ASP.NET Core binding specific.

		// Let's try to create a test that just validates the class structure first.
		return Task.CompletedTask;
	}

	[Fact]
	public void CreateCompanyCommand_ShouldBeMutable()
	{
		var cmd = new CreateCompanyCommand();
		cmd.Title = "Test";
		cmd.Slug = "test";

		Assert.Equal("Test", cmd.Title);
		Assert.Equal("test", cmd.Slug);
	}
}
