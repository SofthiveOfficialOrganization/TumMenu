using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace Infrastructure.Auth;

public sealed class LoggingAuthorizationMiddlewareResultHandler(
	ILogger<LoggingAuthorizationMiddlewareResultHandler> logger
) : IAuthorizationMiddlewareResultHandler
{
	private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

	public async Task HandleAsync(
		RequestDelegate next,
		HttpContext context,
		AuthorizationPolicy policy,
		PolicyAuthorizationResult authorizeResult
	)
	{
		if(!authorizeResult.Succeeded)
		{
			var userName = context.User?.Identity?.Name ?? "(anonymous)";
			var failedRequirements = authorizeResult.AuthorizationFailure?.FailedRequirements
				.Select(r => r.GetType().Name)
				.ToArray() ?? Array.Empty<string>();
			var requiredRoles = policy.Requirements
				.OfType<RolesAuthorizationRequirement>()
				.SelectMany(r => r.AllowedRoles ?? Array.Empty<string>())
				.Distinct()
				.ToArray();

			var userRoles = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value)
				.Concat(context.User.FindAll("role").Select(c => c.Value))
				.Distinct()
				.ToArray();

			var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;

			logger.LogWarning(
				"Authorization failed for {Method} {Path}. Auth={Auth}. User={User}. RequiredRoles=[{RequiredRoles}] UserRoles=[{UserRoles}]",
				context.Request.Method,
				context.Request.Path,
				isAuthenticated,
				userName,
				string.Join(", ", requiredRoles),
				string.Join(", ", userRoles)
			);
		}

		await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
	}
}
