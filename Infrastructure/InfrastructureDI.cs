using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
	public static class InfrastructureDI
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddIdentity<ApplicationUser, ApplicationRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddScoped<IUserContext, HttpUserContext>();
			services.AddScoped<AuditInterceptor>();

			services.AddDbContext<ApplicationDbContext>((sp, opts) =>
			{
				opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
				opts.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
				// Global NoTracking verme; repo zaten kontrol ediyor.
			});

			services.AddScoped(typeof(Application.Abstractions.IRepository<>), typeof(EfRepository<>));
			services.AddScoped<Application.Abstractions.IUnitOfWork, UnitOfWork>();

			return services;
		}

	}
}
