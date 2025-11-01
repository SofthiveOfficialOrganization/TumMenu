using Infrastructure;

namespace TumMenu.Extensions
{
	public static class ServiceExtensions
	{
		public static void AddServices(this WebApplicationBuilder builder)
		{
			var configuration = builder.Configuration;
			var services = builder.Services;

			services
				.AddInfrastructure(configuration);
		}
	}
}
