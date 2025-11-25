using Application.Abstractions;
using MediatR;

namespace Application.Common.Behaviors;

public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork uow)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if(request is not ITransactionalRequest)
            return await next();

        TResponse? resp = default;
        await uow.ExecuteInTransactionAsync(async _ =>
        {
            resp = await next();
        }, ct);

        return resp!;
    }
}