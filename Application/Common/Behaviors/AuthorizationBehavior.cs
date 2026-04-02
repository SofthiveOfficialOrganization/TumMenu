using Application.Abstractions;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(IUserContext userContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (request is IAuthorizedRequest && !userContext.IsAuthenticated)
            throw new UnauthorizedAccessException();

        return await next();
    }
}
