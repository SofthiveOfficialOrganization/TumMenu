using Application;
using Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
			});

			services
				.AddApplication()
				.AddInfrastructure(configuration);

			services.AddControllers();
			services.AddEndpointsApiExplorer();
		}
	}
}
