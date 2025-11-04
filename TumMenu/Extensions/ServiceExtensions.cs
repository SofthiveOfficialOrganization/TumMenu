using Infrastructure;
using Application;
namespace TumMenu.Extensions
{
	public static class ServiceExtensions
	{
		public static void AddServices(this WebApplicationBuilder builder)
		{
			var configuration = builder.Configuration;
			var services = builder.Services;

			services.AddApplication()
				.AddInfrastructure(configuration);

			services.AddControllers();
			services.AddEndpointsApiExplorer();
			services.AddSwaggerGen();
		}
	}
}
