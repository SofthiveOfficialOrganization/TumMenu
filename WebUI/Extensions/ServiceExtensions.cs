using Application;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using WebUI.Services;
using WebUI.Services.Turnstile;
using WebUI.Validators;
using Domain.Entities;

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

				// Use email for login instead of username
				o.User.RequireUniqueEmail = true;
				o.User.AllowedUserNameCharacters = ""; // Disable username validation

				// Password validation with Turkish error messages
				o.Password.RequireNonAlphanumeric = true;
				o.Password.RequireUppercase = true;
				o.Password.RequireLowercase = true;
				o.Password.RequireDigit = true;
				o.Password.RequiredUniqueChars = 1;
			});

			services.Configure<Application.Microservices.Location.LocationMicroserviceOptions>(
				configuration.GetSection("LocationMicroservice"));

			services.AddHttpClient<Application.Microservices.Location.ILocationMicroservice, Application.Microservices.Location.LocationMicroservice>();

			// Add HttpClientFactory for Turnstile API calls
			services.AddHttpClient();

			// Register Turnstile service
			services.AddScoped<ITurnstileService, TurnstileService>();

			// Register EmailTemplateService
			services.AddScoped<IEmailTemplateService, EmailTemplateService>();

			// Register TurnstileService
			services.AddScoped<ITurnstileService, TurnstileService>();

			// Register custom Turkish validators
			services.AddTransient<IPasswordValidator<ApplicationUser>, TurkishPasswordValidator>();
			services.AddTransient<IUserValidator<ApplicationUser>, TurkishUserValidator>();

			services
				.AddApplication()
				.AddInfrastructure(configuration);
		}
	}
}
