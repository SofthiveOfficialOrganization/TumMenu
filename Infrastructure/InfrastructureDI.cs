using Application.Abstractions;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Auth;
using Infrastructure.DataMigration;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
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
			services.AddHttpContextAccessor();
			services.AddMemoryCache();

			services.AddScoped<IUserContext, HttpUserContext>();
			services.AddScoped<AuditInterceptor>();
			services.AddSingleton<IAuthorizationMiddlewareResultHandler, LoggingAuthorizationMiddlewareResultHandler>();

			services.AddDbContext<ApplicationDbContext>((sp, opts) =>
			{
				opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
				opts.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
			});

			services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

			services.AddIdentity<ApplicationUser, ApplicationRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

			services.AddScoped<IUnitOfWork, UnitOfWork>();
			services.AddScoped<IStorageService, LocalStorageService>();

			services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

			services.AddSingleton<MigrationTableCatalog>();
			services.AddScoped<IDataMigrationService, DataMigrationService>();

			return services;
		}
	}
}
