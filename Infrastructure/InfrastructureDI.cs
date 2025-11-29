using Application.Abstractions;
using Domain.Entities;
using Infrastructure.Auth;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastructure
{
	public static class InfrastructureDI
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddHttpContextAccessor();

			services.AddScoped<IUserContext, HttpUserContext>();
			services.AddScoped<AuditInterceptor>();

			services.AddDbContext<ApplicationDbContext>((sp, opts) =>
			{
				opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
				opts.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
			});

			services.AddIdentity<ApplicationUser, ApplicationRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();
			services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, AppClaimsPrincipalFactory>();

			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));

			return services;
		}
	}
}
