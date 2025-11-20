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

			services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
			services.AddScoped<IJwtTokenService, JwtTokenService>();

			var jwt = configuration.GetSection("Jwt");
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!));

			services
				.AddAuthentication(options =>
				{
					options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
					options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
				})
				.AddJwtBearer(o =>
				{
					o.TokenValidationParameters = new TokenValidationParameters
					{
						ValidIssuer = configuration["Jwt:Issuer"],      // "TumMenu"
						ValidAudience = configuration["Jwt:Audience"],  // "TumMenu.Api"
						IssuerSigningKey = new SymmetricSecurityKey(
							Encoding.UTF8.GetBytes(configuration["Jwt:Secret"])
						),
						ValidateIssuer = true,
						ValidateAudience = true,
						ValidateIssuerSigningKey = true,
						ValidateLifetime = true,
						ClockSkew = TimeSpan.FromSeconds(30)
					};
				});

			services.AddScoped<IUnitOfWork, UnitOfWork>();

			services.AddScoped(typeof(Application.Abstractions.IRepository<>), typeof(EfRepository<>));

			return services;
		}
	}
}
