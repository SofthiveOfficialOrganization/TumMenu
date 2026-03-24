using Application;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebUI.Services;

namespace WebUI.Extensions
{
	public static class ServiceExtensions
	{
		public static void AddServices(this WebApplicationBuilder builder)
		{
			var services = builder.Services;
			var configuration = builder.Configuration;

			services.Configure<IdentityOptions>(o =>
			{
				o.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
				o.Lockout.MaxFailedAccessAttempts = 5;
				o.Lockout.AllowedForNewUsers = true;
				o.SignIn.RequireConfirmedAccount = true;
				o.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier;

			});

			services.Configure<Application.Microservices.Location.LocationMicroserviceOptions>(
				configuration.GetSection("LocationMicroservice"));

			services.AddHttpClient<Application.Microservices.Location.ILocationMicroservice, Application.Microservices.Location.LocationMicroservice>();

			// Register EmailTemplateService
			services.AddScoped<IEmailTemplateService, EmailTemplateService>();

			services
				.AddApplication()
				.AddInfrastructure(configuration);
		}
	}
}
