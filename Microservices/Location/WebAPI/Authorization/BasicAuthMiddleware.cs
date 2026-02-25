using System.Text;
using Amazon.LocationService.Model;
using WebAPI.Constants;
using WebAPI.Exceptions;
using WebAPI.Models.Concrete;

namespace WebAPI.Authorization;

public class BasicAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
	private readonly RequestDelegate _next = next;
	private readonly AuthOptions _authOptions = configuration.GetSection("Authentication").Get<AuthOptions>() ?? throw new InternalServerException(AppMessages.SERVER_ERROR_TITLE);

	public async Task Invoke(HttpContext context)
	{
		//if (!context.Request.Headers.ContainsKey("Authorization"))
		//{
		//    throw new AuthorizationException(AppMessages.MISSING_AUTH_HEADER);
		//}

		//var authHeader = context.Request.Headers["Authorization"].ToString();
		//if (!authHeader.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
		//{
		//    throw new AuthorizationException(AppMessages.INVALID_AUTH_HEADER);
		//}

		//var encodedCredentials = authHeader["Basic ".Length..].Trim();
		//string decodedCredentials;

		//try
		//{
		//    decodedCredentials = Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
		//}
		//catch (FormatException)
		//{
		//    throw new AuthorizationException(AppMessages.INVALID_AUTH_HEADER);
		//}

		//var credentials = decodedCredentials.Split(':', 2);
		//if (credentials.Length != 2 || credentials[0] != _authOptions.Username || credentials[1] != _authOptions.Password)
		//{
		//    throw new AuthorizationException(AppMessages.INVALID_CREDENTIALS);
		//}

		await _next(context);
	}
}
