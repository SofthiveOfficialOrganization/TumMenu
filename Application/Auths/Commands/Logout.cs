using Application.Abstractions;
using Application.Common.Exceptions;
using MediatR;

namespace Application.Auths.Commands;

public sealed record LogoutCommand : IRequest<Unit>;

public sealed class LogoutHandler(
    IUserContext ctx,
    IJwtTokenService tokens
) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand req, CancellationToken ct)
    {
        if(!ctx.IsAuthenticated || string.IsNullOrEmpty(ctx.UserId))
            throw new UnauthorizedAppException("Giriş gerekli.");

        await tokens.RevokeAsync(ctx.UserId!, ct);
        return Unit.Value;
    }
}