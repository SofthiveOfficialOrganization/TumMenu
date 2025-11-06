using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Application.Common.Behaviors;

public sealed class RequestLoggingBehavior<TRequest, TResponse>(ILogger<RequestLoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly ILogger<RequestLoggingBehavior<TRequest, TResponse>> _logger = logger;

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		var name = typeof(TRequest).Name;
		_logger.LogInformation("Handling {RequestName}", name);
		var sw = Stopwatch.StartNew();

		try
		{
			var response = await next();
			sw.Stop();
			_logger.LogInformation("Handled {RequestName} in {Elapsed}ms", name, sw.ElapsedMilliseconds);
			return response;
		}
		catch(Exception ex)
		{
			sw.Stop();
			_logger.LogError(ex, "Error in {RequestName} after {Elapsed}ms", name, sw.ElapsedMilliseconds);
			throw;
		}
	}
}
