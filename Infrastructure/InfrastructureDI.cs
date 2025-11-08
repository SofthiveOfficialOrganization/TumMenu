using System.Text;
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

namespace Infrastructure
{
	public static class InfrastructureDI
	{
		public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
		{
			// IHttpContextAccessor (HttpUserContext için)
			services.AddHttpContextAccessor();

			// Audit Interceptor / UserContext
			services.AddScoped<IUserContext, HttpUserContext>();
			services.AddScoped<AuditInterceptor>();

			// DbContext (+ interceptor)
			services.AddDbContext<ApplicationDbContext>((sp, opts) =>
			{
				opts.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
				opts.AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
			});

			// Identity
			services.AddIdentity<ApplicationUser, ApplicationRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			// JWT Options + Token service
			services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
			services.AddScoped<IJwtTokenService, JwtTokenService>();

			// JWT Bearer auth
			var jwt = configuration.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));

			services
				.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(o =>
				{
					o.TokenValidationParameters = new TokenValidationParameters
					{
						ValidIssuer = jwt["Issuer"],
						ValidAudience = jwt["Audience"],
						IssuerSigningKey = key,
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateIssuerSigningKey = true,
						ValidateLifetime = true,
						ClockSkew = TimeSpan.FromSeconds(30)
					};
				});

			// UoW
			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddScoped(typeof(Application.Abstractions.IRepository<>), typeof(EfRepository<>));

			return services;
		}
	}
}
