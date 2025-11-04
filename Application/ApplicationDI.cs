using System.Reflection;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
	public static IServiceCollection AddApplication(this IServiceCollection services)
	{
		var asm = Assembly.GetExecutingAssembly();

		services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(asm));
		services.AddValidatorsFromAssembly(asm);
		var mapsterConfig = new TypeAdapterConfig();
		mapsterConfig.Scan(asm);
		services.AddSingleton(mapsterConfig);
		services.AddScoped<IMapper, ServiceMapper>();

		// Validation pipeline  
		services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

		return services;
	}
}

// Pipeline
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	private readonly IEnumerable<IValidator<TRequest>> _validators;
	public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		if(_validators.Any())
		{
			var ctx = new ValidationContext<TRequest>(request);
			var errors = (await Task.WhenAll(_validators.Select(v => v.ValidateAsync(ctx, ct))))
				.SelectMany(r => r.Errors)
				.Where(e => e != null)
				.ToList();

			if(errors.Count > 0)
				throw new FluentValidation.ValidationException(errors);
		}
		return await next();
	}
}
