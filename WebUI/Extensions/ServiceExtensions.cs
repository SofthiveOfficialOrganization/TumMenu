using Application;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

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

			services
				.AddApplication()
				.AddInfrastructure(configuration);
		}
	}
}
