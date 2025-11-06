using Application.Common.Exceptions;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(
	IEnumerable<IValidator<TRequest>> validators)
	: IPipelineBehavior<TRequest, TResponse>
	where TRequest : notnull
{
	public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
	{
		if(!validators.Any())
			return await next();

		var context = new ValidationContext<TRequest>(request);

		var failures = (await Task.WhenAll(
				validators.Select(v => v.ValidateAsync(context, ct))))
			.SelectMany(r => r.Errors)
			.Where(e => e is not null)
			.ToList();

		if(failures.Count > 0)
		{
			var errors = failures
				.GroupBy(f => f.PropertyName)
				.ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).Distinct().ToArray());

			throw new ValidationAppException(errors);
		}

		return await next();
	}
}
